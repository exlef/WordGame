using System;
using System.Collections.Generic;
using System.Linq;
using DTT.MinigameBase;
using UnityEngine;
using WordGame;

namespace DTT.WordConnect
{
    /// <summary>
    /// Managing script which ties together all the data and UI, controls start, pause and finish behaviour.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class WordConnectManager : MonoBehaviour, IMinigame<WordConnectConfigurationData, WordConnectResult>
    {
        /// <summary>
        /// The singleton instance of this script which other behviours can use to attach to events.
        /// </summary>
        public static WordConnectManager Instance { get; set; } 

        /// <summary>
        /// The config file that will be used in this game.
        /// </summary>
        public WordConnectConfigurationData Configuration { get; private set; }

        /// <summary>
        /// The level number currently being played.
        /// </summary>
        public int CurrentLevelNumber { get; private set; } = 1;

        /// <summary>
        /// Class which holds the state of the word connect game.
        /// </summary>
        public WordConnectState WordConnectState { get; private set; }

        /// <summary>
        /// Letters which are available to spell the words in the game.
        /// </summary>
        public List<LetterInput> AvailableLetters { get; private set; }

        /// <summary>
        /// Project-level tuning values for scoring and presentation.
        /// </summary>
        [SerializeField]
        private GameConfig _gameConfig;

        /// <summary>
        /// Project-level tuning values for scoring and presentation.
        /// </summary>
        public GameConfig GameConfig => _gameConfig;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsGameActive { get; private set; }

        /// <summary>
        /// Event is invoked whenever the WordConnectState changes. (input changes, word is found or hinted)
        /// Any UI needing this data can listen to this event for changes.
        /// </summary>
        public Action<WordConnectState> StateUpdated;

        /// <summary>
        /// Is called once the the game manager has initialized its variables.
        /// Listen to this event if you require to do something before the game grid is built.
        /// </summary>
        public event Action Initialized;

        /// <summary>
        /// Is called once the game grid should be built.
        /// </summary>
        public event Action BuildGame;
         
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event Action Started;

        /// <summary>
        /// Is called when the game has been paused.
        /// </summary>
        public event Action Paused;

        /// <summary>
        /// Is called when the game has been continued.
        /// </summary>
        public event Action Continued;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event Action<WordConnectResult> Finish;

        /// <summary>
        /// Event called right before ending the game.
        /// </summary>
        public event Action Cleanup;

        /// <summary>
        /// Initialize the singleton instance.
        /// </summary>
        private void Awake() => Instance = this;

        /// <summary>
        /// Sets the level number currently being played.
        /// </summary>
        /// <param name="levelNumber">The selected level number.</param>
        public void SetCurrentLevelNumber(int levelNumber) => CurrentLevelNumber = Mathf.Max(1, levelNumber);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config">The config file used to start the game.</param>
        public void StartGame(WordConnectConfigurationData config)
        {
            if (IsGameActive)
                return;

            // Set game related elements.
            IsGameActive = true;
            IsPaused = false;
            Configuration = config;

            // Create a new game state object.
            WordConnectState = new WordConnectState();
            WordConnectState.SetHintBalance(Configuration.LetterHintsAvailable, Configuration.WordHintsAvailable, Configuration.DescriptionHintsAvailable);

            // Initialize two new lists.
            List<string> wordsInCrossword = new List<string>();
            List<WordHintPair> wordsHintsInCrossword = new List<WordHintPair>();

            // Get all placed words from the configuration object and add them to the lists.
            foreach (WordVector wordVector in Configuration.WordVectors)
            {
                string normalizedWord = KurmanjiTextUtility.NormalizeWord(wordVector.Word);
                wordsInCrossword.Add(normalizedWord);
                wordsHintsInCrossword.Add(Configuration.GetWordHintPair(normalizedWord));
            }

            // Update the game state object with the words currently in the game and build the hint options with the given words.
            WordConnectState.SetAvailableWords(wordsInCrossword);
            WordConnectState.SetTotalLevelLetters(wordsInCrossword.Sum(word => word.Length));
            WordConnectState.SetHintOptions(wordsHintsInCrossword);

            // Get the necessary letters from the words list.
            List<char> availableLetters = GetAllLettersFromWords(WordConnectState.WordsInCrossword);
            SetAvailableLetters(availableLetters);

            // Invoke the initialization event. 
            Initialized?.Invoke();
            // Invoke the build event.
            BuildGame?.Invoke();
            // Invoke the start of the game.
            Started?.Invoke();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Pause()
        {
            if (!IsGameActive)
                return;

            // Set game related elements.
            IsPaused = true;

            // Invoke the pause of the game.
            Paused?.Invoke();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Continue()
        {
            if (!IsGameActive)
                return;

            // Set game related elements.
            IsPaused = false;

            // Invoke the end of the game.
            Continued?.Invoke();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ForceFinish()
        {
            if (!IsGameActive)
                return;

            // Stops the game.
            ForceStop();

            // Generate a results struct.
            int stars = GetStarsForAssistRatio(WordConnectState.AssistRatio);
            WordConnectResult results = new WordConnectResult(stars, WordConnectState.HintPointsUsed, WordConnectState.TotalLevelLetters);

            // Invoke the end of the game.
            Finish?.Invoke(results);
        }

        /// <summary>
        /// Is called when the user want to force stop the game.
        /// </summary>
        public void ForceStop()
        {
            // Set game related elements.
            IsGameActive = false;
            IsPaused = false;

            Cleanup?.Invoke();
        }

        /// <summary>
        /// Is called when the user wants to restart the current game.
        /// </summary>
        public void ForceRestart()
        {
            // Stop the ongoing game.
            ForceStop();

            // Restart using the current configuration.
            StartGame(Configuration);
        }

        /// <summary>
        /// Checks whether the letter input is valid and handles accordingly.
        /// </summary>
        /// <param name="letterInput">Information on what letter was pressed.</param>
        public void HandleLetterInput(LetterInput letterInput)
        {
            // Check if letter has a valid char.
            if (!char.IsLetter(letterInput.letter)) return;

            // Get the current sequence of letters which were pressed.
            List<LetterInput> currentLetterSequence = WordConnectState.CurrentWordInput;

            // If the previous letter was inputted, remove the input. (erase the last letter)
            if (currentLetterSequence.Count > 1 && currentLetterSequence[currentLetterSequence.Count - 2].id == letterInput.id)
            {
                WordConnectState.RemoveLastLetter();
                StateUpdated?.Invoke(WordConnectState);
            }
            // If this LetterInput isn't already added in the sequence (each letter can only be use once), add it to the end.
            else if (!WordConnectState.CurrentWordInput.Contains(letterInput))
            {
                WordConnectState.AddLetterInput(letterInput);
                StateUpdated?.Invoke(WordConnectState);
            }
        }

        /// <summary>
        /// Reveals a single letter of a word.
        /// </summary>
        public void RevealLetterHint()
        {
            // Check if there are any hint options left.
            if (WordConnectState.HintOptions.Count == 0)
                // No hints options left.
                return;

            // Get a random word hint option object from the word connect state.
            HintOption hint = WordConnectState.HintOptions[UnityEngine.Random.Range(0, WordConnectState.HintOptions.Count)];

            // the index list representing the letter indexes of the word which are available to reveal.
            List<int> availableLetters = new List<int>();

            // Get all the letters of this word that have not yet been revealed or found.
            for (int i = 0; i < hint.LetterRevealed.Count; i++)
            {
                // Check if this letter has been revealed.
                if (!hint.LetterRevealed[i])
                {
                    // Check if this letter has overlap.
                    if (Configuration.LetterHasOverlap(hint.Word, i))
                    {
                        // Get the other word that this letter overlaps.
                        string overlappingWord = Configuration.GetLetterOverlap(hint.Word, i).Item1;
                        // If the overlapped word is already found this is not a viable letter to reveal as it has already been found!
                        if (!WordConnectState.CorrectlyAddedWords.Any(word => KurmanjiTextUtility.WordsEqual(word, overlappingWord)))
                        {
                            availableLetters.Add(i);
                        }
                    }
                    // If the letter doesnt have overlap it is a viable letter to reveal.
                    else
                    {
                        availableLetters.Add(i);
                    }
                }
            }

            // Get a random index of one of the available letters to reveal.
            int chosenHintLetter = availableLetters[UnityEngine.Random.Range(0, availableLetters.Count)];
            // Reveal the chosen letter.
            WordConnectState.SetHintLetterRevealed(hint, chosenHintLetter);

            // Check if the chosen letter has an overlapping word in the grid.
            if (Configuration.LetterHasOverlap(hint.Word, chosenHintLetter))
            {
                // Gets the word string for the overlapping word and the index of the letter which overlaps.
                (string, int) wordLetterIndex = Configuration.GetLetterOverlap(hint.Word, chosenHintLetter);
                // If this word is not hintable skip the next part.
                if (WordConnectState.HintOptions.Any(x => KurmanjiTextUtility.WordsEqual(x.Word, wordLetterIndex.Item1)))
                {
                    // Find the HintOption object for the overlapping word.
                    HintOption overlappingHint = WordConnectState.HintOptions.Find(x => KurmanjiTextUtility.WordsEqual(x.Word, wordLetterIndex.Item1));
                    // Set the overlapping letter index to revealed.
                    WordConnectState.SetHintLetterRevealed(overlappingHint, wordLetterIndex.Item2);
                }
            }
            // Subtract one of the letter hint balance.
            WordConnectState.UseLetterHint();
            WordConnectState.AddHintPoints(GetLetterHintCost());
            // Game state updated, invoke the event for UI to respond accordingly.
            StateUpdated?.Invoke(WordConnectState);
            // If this was the last available letter of this hint remove it *after* the StateUpdated event so UI can update their state before the hint is removed completely.
            if (availableLetters.Count == 1)
            {
                // All letters have been revealed or found, set this word to found.
                WordConnectState.AddFoundWord(hint.Word);
                WordConnectState.RemoveHintOption(hint);
            }
            // Check if the game has been completed.
            if (CheckGameComplete())
                ForceFinish();
        }

        /// <summary>
        /// Reveals an entire word.
        /// </summary>
        public void RevealWordHint()
        {
            // Check if there are any hint options left.
            if (WordConnectState.HintOptions.Count == 0)
                return;

            HintOption hint;
            // Prioritization, find the longest word to reveal.
            if (Configuration.PrioritizeLongestWordHint)
            {
                // Set the first hint as the selected hint.
                hint = WordConnectState.HintOptions[0];

                // Loop over all other hints to check if they have more unrevealed letters.
                for (int i = 1; i < WordConnectState.HintOptions.Count; i++)
                {
                    // Count the amount of letters that haven't been revealed yet.
                    int hintableLettersInWord = WordConnectState.HintOptions[i].LetterRevealed.Count(revealed => !revealed);
                    // If this hint has more unrevealed letters than the selected hint, set this hint as the new selected hint.
                    if (hintableLettersInWord > hint.LetterRevealed.Count(revealed => !revealed))
                        hint = WordConnectState.HintOptions[i];
                }
            }
            else
            {
                // Get a random word hint option object from the word connect state.
                hint = WordConnectState.HintOptions[UnityEngine.Random.Range(0, WordConnectState.HintOptions.Count)];
            }
            int wordHintCost = GetWordHintCost(hint.Word.Length);
            // Set the chosen hint to revealed.
            WordConnectState.SetHintWordRevealed(hint);

            // Loop over all the letters in the chosen word hint.
            for (int i = 0; i < hint.Word.Length; i++)
            {
                // Check if the letter overlaps with any other word in the grid.
                if (Configuration.LetterHasOverlap(hint.Word, i))
                {
                    // Gets the word string for the overlapping word and the index of the letter which overlaps.
                    (string, int) wordLetterIndex = Configuration.GetLetterOverlap(hint.Word, i);
                    // If this word is not a hintable, skip the next part.
                    if (!WordConnectState.HintOptions.Any(x => KurmanjiTextUtility.WordsEqual(x.Word, wordLetterIndex.Item1))) continue;
                    // Find the HintOption object for the overlapping word.
                    HintOption overlappingHint = WordConnectState.HintOptions.Find(x => KurmanjiTextUtility.WordsEqual(x.Word, wordLetterIndex.Item1));
                    // Set the overlapping letter index to revealed.
                    WordConnectState.SetHintLetterRevealed(overlappingHint, wordLetterIndex.Item2);
                }
            }
            // Subtract one of the word hint balance.
            WordConnectState.UseWordHint();
            WordConnectState.AddHintPoints(wordHintCost);
            // Game state updated, invoke the event for UI to respond accordingly.
            StateUpdated?.Invoke(WordConnectState);

            // Check if the game has been completed.
            if (CheckGameComplete())
                ForceFinish();
        }
        /// <summary>
        /// Reveals the description of a word.
        /// </summary>
        public void RevealDescriptiveHint()
        {
            // If there is already an active hint, cancel.
            if (WordConnectState.CurrentRevealedDescriptiveHint != null)
                return;

            // Get all hint options which haven't had their description revealed.
            List<HintOption> availableDescriptiveHints = WordConnectState.HintOptions.FindAll(hint => !hint.DescriptionRevealed);

            if (availableDescriptiveHints.Count == 0)
                // There are no available descriptive hints available, return.
                return;

            // Get a random hint object from the list of available hints.
            HintOption chosenHint = availableDescriptiveHints[UnityEngine.Random.Range(0, availableDescriptiveHints.Count)];
            // Set the chosen hint.
            WordConnectState.SetDescriptiveHint(chosenHint);

            // Subtract one of the descriptive hints balance.
            WordConnectState.UseDescriptiveHint();
            WordConnectState.AddHintPoints(GetDescriptionHintCost());
            // Game state updated, invoke the event for UI to respond accordingly.
            StateUpdated?.Invoke(WordConnectState);
        }


        /// <summary>
        /// Checks whether the inputted word is valid and is in this game layout.
        /// </summary>
        public void SubmitWord()
        {
            // Word input is empty or null.
            if (WordConnectState == null || WordConnectState.CurrentWordInput == null || WordConnectState.CurrentWordInput.Count == 0) return;

            string spelledWord = KurmanjiTextUtility.NormalizeWord(WordConnectState.GetCurrentWordInput());
            // Check if the crossword contains the spelled word.
            if (WordConnectState.WordsInCrossword.Any(word => KurmanjiTextUtility.WordsEqual(word, spelledWord)))
            {
                // Check if this word hasn't already been found.
                if (WordConnectState.CorrectlyAddedWords.Any(word => KurmanjiTextUtility.WordsEqual(word, spelledWord)))
                {
                    // Word has already been found.
                }
                else
                {
                    // Word has been found, add the found word to the state object.
                    WordConnectState.AddFoundWord(spelledWord);
                    // Remove the word from the available hint options.
                    WordConnectState.RemoveHintOption(spelledWord);
                }
            }

            // Clear the input sequence.
            WordConnectState.ClearInput();
            StateUpdated?.Invoke(WordConnectState);

            if (CheckGameComplete())
                // All words in the game have been found, finish the game.
                ForceFinish();
        }

        /// <summary>
        /// Checks if all words in the game have been found.
        /// </summary>
        /// <returns>Bool whether all words have been found.</returns>
        private bool CheckGameComplete()
        {
            // If no words have been found, return.
            if (WordConnectState.CorrectlyAddedWords.Count == 0)
                return false;

            // Get all the words in the game.
            List<string> wordsInCrossword = new List<string>(WordConnectState.WordsInCrossword);

            // Remove all words that have been added from the list.
            foreach (string word in WordConnectState.CorrectlyAddedWords)
                wordsInCrossword.RemoveAll(crosswordWord => KurmanjiTextUtility.WordsEqual(crosswordWord, word));

            // Remove all words that have been completely revealed by hinting.
            foreach(HintOption hint in WordConnectState.CompletelyHintedWords)
                wordsInCrossword.RemoveAll(crosswordWord => KurmanjiTextUtility.WordsEqual(crosswordWord, hint.Word));

            // If the list has no words left, this indicates all words have either been found or completely hinted.
            return wordsInCrossword.Count == 0;
        }

        /// <summary>
        /// Returns a list of characters which are necessary to spell all words in the game.
        /// </summary>
        /// <param name="words">List of strings containing the words.</param>
        /// <returns>List of chars required to spell all words in the given list</returns>
        private List<char> GetAllLettersFromWords(List<string> words)
        {
            List<char> neededLetters = new List<char>();

            // All letters in first word can be added.
            foreach (char letter in words[0])
                neededLetters.Add(letter);

            // Loop over the remaining words.
            for (int i = 1; i < words.Count; i++)
            {
                foreach (char letter in words[i])
                {
                    if (neededLetters.Contains(letter))
                    {
                        // There is already at least 1 occurence of this letter in the list.
                        // Count the amount of times this letter appears in the letter list and in the current word.
                        int unaddedLetterOccurences = words[i].Count(letterInWord => (letterInWord == letter));
                        int addedLetterOccurences = neededLetters.Count(addedLetter => (addedLetter == letter));

                        // If the letter appears more times in the unadded word than the list we need more of this letter to spell all words. Add the remainder.
                        if (unaddedLetterOccurences > addedLetterOccurences)
                        {
                            for (int l = 0; l < unaddedLetterOccurences - addedLetterOccurences; l++)
                                neededLetters.Add(letter);
                        }
                    }
                    else
                    {
                        // The current letter list doesnt have the current letter. Add it.
                        neededLetters.Add(letter);
                    }
                }
            }
            return neededLetters;
        }

        /// <summary>
        /// Gets the configured description hint cost, using the default if no game config is assigned.
        /// </summary>
        private int GetDescriptionHintCost() => _gameConfig ? _gameConfig.DescriptionHintCost : GameConfig.DefaultDescriptionHintCost;

        /// <summary>
        /// Gets the configured letter hint cost, using the default if no game config is assigned.
        /// </summary>
        private int GetLetterHintCost() => _gameConfig ? _gameConfig.LetterHintCost : GameConfig.DefaultLetterHintCost;

        /// <summary>
        /// Gets the configured word hint cost, using the default if no game config is assigned.
        /// </summary>
        /// <param name="wordLength">The length of the revealed word.</param>
        private int GetWordHintCost(int wordLength) => _gameConfig
            ? _gameConfig.GetWordHintCost(wordLength)
            : Mathf.Max(GameConfig.DefaultMinWordHintCost, wordLength);

        /// <summary>
        /// Converts assist usage into the earned star count.
        /// </summary>
        /// <param name="assistRatio">The used hint points divided by total level letters.</param>
        private int GetStarsForAssistRatio(float assistRatio)
        {
            if (_gameConfig)
                return _gameConfig.GetStarsForAssistRatio(assistRatio);

            if (assistRatio <= GameConfig.DefaultThreeStarMaxAssistRatio)
                return 3;

            if (assistRatio <= GameConfig.DefaultTwoStarMaxAssistRatio)
                return 2;

            return 1;
        }

        /// <summary>
        /// Populates the LetterInput array whch represents which inputs are valid in this game.
        /// </summary>
        /// <param name="letters">List of characters representing the letters which we need to spell the words.</param>
        private void SetAvailableLetters(List<char> letters)
        {
            AvailableLetters = new List<LetterInput>();

            for (int i = 0; i < letters.Count; i++)
            {
                AvailableLetters.Add(new LetterInput(i, letters[i]));
            }
        }
    }
}

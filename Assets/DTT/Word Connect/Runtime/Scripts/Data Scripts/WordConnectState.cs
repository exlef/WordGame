using System.Collections.Generic;

namespace DTT.WordConnect
{
    /// <summary>
    /// Class which holds all the data of the current game which is being played.
    /// </summary>
    public class WordConnectState
    {
        /// <summary>
        /// The sequence of LetterInput objects which reprent the letters that have been inputted in order.
        /// </summary>
        public List<LetterInput> CurrentWordInput { get; private set; }

        /// <summary>
        /// All the words which are in the game. 
        /// </summary>
        public List<string> WordsInCrossword { get; private set; }

        /// <summary>
        /// The words in the game which have been found.
        /// </summary>
        public List<string> CorrectlyAddedWords { get; private set; }

        /// <summary>
        /// List of available word to hint at.
        /// </summary>
        public List<HintOption> HintOptions { get; private set; }

        /// <summary>
        /// List of words which have been completely hinted at, and are no longer viable options.
        /// </summary>
        public List<HintOption> CompletelyHintedWords { get; private set; }

        /// <summary>
        /// The current active descriptive hint.
        /// </summary>
        public HintOption CurrentRevealedDescriptiveHint { get; private set; }

        /// <summary>
        /// The amount of hints the player has left which reveal an entire word.
        /// </summary>
        public int WordHintBalance { get; private set; }

        /// <summary>
        /// The amount of hints the player has left which reveal a single letter.
        /// </summary>
        public int LetterHintBalance { get; private set; }

        /// <summary>
        /// The amount of hints the player has left which display a description of a word.
        /// </summary>
        public int DescriptiveHintBalance { get; private set; }

        /// <summary>
        /// The amount of score gained by finding words.
        /// </summary>
        public int CurrentScore { get; private set; } = 0;

        /// <summary>
        /// The amount of score gained by streaks.
        /// </summary>
        public int CurrentStreakBonusScore { get; private set; } = 0;

        /// <summary>
        /// Counter for the current streak of correct answers.
        /// </summary>
        public int CorrectAnswerStreak { get; private set; } = 0;

        /// <summary>
        /// Builds an empty WordConnectState object.
        /// </summary>
        public WordConnectState()
        {
            CurrentWordInput = new List<LetterInput>();
            WordsInCrossword = new List<string>();
            CorrectlyAddedWords = new List<string>();
            HintOptions = new List<HintOption>();
            CompletelyHintedWords = new List<HintOption>();
        }

        /// <summary>
        /// Pushes the LetterInput onto the CurrentWordInput list.
        /// </summary>
        /// <param name="letter">The LetterInput object which will be appended.</param>
        public void AddLetterInput(LetterInput letter) => CurrentWordInput.Add(letter);

        /// <summary>
        /// Removes the last element of the CurrentWordInput list.
        /// </summary>
        public void RemoveLastLetter() => CurrentWordInput.RemoveAt(CurrentWordInput.Count - 1);

        /// <summary>
        /// Sets the words which are in the game.
        /// </summary>
        /// <param name="availableWords">List of strings of all words in the game.</param>
        public void SetAvailableWords(List<string> availableWords) => WordsInCrossword = availableWords;

        /// <summary>
        /// Sets the balace of all hint options available in this game. (letter hint, word hint and descriptive hint)
        /// </summary>
        /// <param name="letterHints">The amount of letter hints available in this game.</param>
        /// <param name="wordHints">The amount of word hints available in this game.</param>
        /// <param name="descriptiveHints">The amount of descriptive hints available in this game.</param>
        public void SetHintBalance(int letterHints, int wordHints, int descriptiveHints)
        {
            LetterHintBalance = letterHints;
            WordHintBalance = wordHints;
            DescriptiveHintBalance = descriptiveHints;
        }

        /// <summary>
        /// Subtracts 1 from the letter hint balance.
        /// </summary>
        public void UseLetterHint() => LetterHintBalance -= 1;

        /// <summary>
        /// Subtracts 1 from the word hint balance.
        /// </summary>
        public void UseWordHint() => WordHintBalance -= 1;

        /// <summary>
        /// Subtracts 1 from the descriptive hint balance.
        /// </summary>
        public void UseDescriptiveHint() => DescriptiveHintBalance -= 1;

        /// <summary>
        /// Adds score to the current score counter.
        /// </summary>
        /// <param name="amount">The amount to add to the score.</param>
        public void AddScore(int amount) => CurrentScore += amount;

        /// <summary>
        /// Adds score to the streak score counter.
        /// </summary>
        /// <param name="amount">The amount to add to the score.</param>
        public void AddStreakScore(int amount) => CurrentStreakBonusScore += amount;

        /// <summary>
        /// Increases the streak counter.
        /// </summary>
        /// <param name="amount">The amount to increase the streak by.</param>
        public void IncreaseStreak(int amount) => CorrectAnswerStreak += amount;

        /// <summary>
        /// Clears the streak counter.
        /// </summary>
        public void ClearStreak() => CorrectAnswerStreak = 0;

        /// <summary>
        /// Builds the available hint options array from a set of words.
        /// </summary>
        /// <param name="availableWords">List of WordHintPair objects which are active in this game.</param>
        public void SetHintOptions(List<WordHintPair> availableWords)
        {
            foreach (WordHintPair word in availableWords)
                HintOptions.Add(new HintOption(word.Word, word.Hint));
        }

        /// <summary>
        /// Removes a hint option using the word the hint option object is representing.
        /// </summary>
        /// <param name="word">The word which the hint option represents.</param>
        public void RemoveHintOption(string word)
        {
            // Find the hint with the given word and remove it from the list.
            HintOption hint = HintOptions.Find(x => x.Word == word);
            HintOptions.Remove(hint);
            if (CurrentRevealedDescriptiveHint != null && CurrentRevealedDescriptiveHint.Word == word) 
                CurrentRevealedDescriptiveHint = null;
        }

        /// <summary>
        /// Removes the given hintoption from the available hints list.
        /// </summary>
        /// <param name="hint">The HintOption object to remove from the available hints list.</param>
        public void RemoveHintOption(HintOption hint) => HintOptions.Remove(hint);

        /// <summary>
        /// Sets the given index to true indicating the letter of the word at the index has been shown.
        /// </summary>
        /// <param name="hint">The hint object that is being used.</param>
        /// <param name="letterIndex">The letter index of the word.</param>
        public void SetHintLetterRevealed(HintOption hint, int letterIndex)
        {
            int hintIndex = HintOptions.IndexOf(hint);

            HintOptions[hintIndex].SetLetterRevealed(letterIndex);
            if (HintOptions[hintIndex].LetterRevealed.TrueForAll(x => x))
            {
                // All letters in this word hint object have been revealed. This is no longer a valid hint option, move it the completed list.
                CompletelyHintedWords.Add(HintOptions[hintIndex]);
                HintOptions.Remove(HintOptions[hintIndex]);
            }
        }

        /// <summary>
        /// Sets all letters of a HintOption to revealed.
        /// </summary>
        /// <param name="hint">The hint object which should be revealed.</param>
        public void SetHintWordRevealed(HintOption hint)
        {
            hint.SetWordRevealed();
            // The entire hintOption has been revealed, move it the completed list.
            CompletelyHintedWords.Add(hint);
            AddFoundWord(hint.Word);
            HintOptions.Remove(hint);
        }

        /// <summary>
        /// Sets a given hint object to the active descriptive hint.
        /// </summary>
        /// <param name="hint">The hint which will have its description revealed.</param>
        public void SetDescriptiveHint(HintOption hint)
        {
            hint.SetDescriptionRevealed();
            CurrentRevealedDescriptiveHint = hint;
        }

        /// <summary>
        /// Adds a word to the found word list.
        /// </summary>
        /// <param name="foundWord">The word that has been found.</param>
        public void AddFoundWord(string foundWord) => CorrectlyAddedWords.Add(foundWord);

        /// <summary>
        /// Clears the input sequence.
        /// </summary>
        public void ClearInput() => CurrentWordInput.Clear();

        /// <summary>
        /// Retrieves the LetterInput sequence in string format.
        /// </summary>
        /// <returns>String format of the current letter input sequence.</returns>
        public string GetCurrentWordInput()
        {
            string sequence = string.Empty;

            foreach (LetterInput letterInput in CurrentWordInput) 
                sequence += letterInput.letter;

            return sequence.ToUpper();
        }
    }
}

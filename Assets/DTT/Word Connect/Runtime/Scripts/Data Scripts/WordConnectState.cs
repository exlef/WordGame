using System;
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
        /// The sum of all letters in the placed words for the level.
        /// </summary>
        public int TotalLevelLetters { get; private set; }

        /// <summary>
        /// The score cost of all assists the player used in the level.
        /// </summary>
        public int HintPointsUsed { get; private set; }

        /// <summary>
        /// The ratio of used assist points to level letters.
        /// </summary>
        public float AssistRatio => TotalLevelLetters <= 0 ? 0f : (float)HintPointsUsed / TotalLevelLetters;

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
        public void SetAvailableWords(List<string> availableWords)
        {
            WordsInCrossword = new List<string>();
            foreach (string word in availableWords)
                WordsInCrossword.Add(KurmanjiTextUtility.NormalizeWord(word));
        }

        /// <summary>
        /// Sets the total letter count used to normalize assist usage.
        /// </summary>
        /// <param name="totalLevelLetters">The total amount of letters in all placed words.</param>
        public void SetTotalLevelLetters(int totalLevelLetters) => TotalLevelLetters = Math.Max(0, totalLevelLetters);

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
        /// Adds assist points to the level total.
        /// </summary>
        /// <param name="amount">The amount of assist points to add.</param>
        public void AddHintPoints(int amount) => HintPointsUsed += Math.Max(0, amount);

        /// <summary>
        /// Builds the available hint options array from a set of words.
        /// </summary>
        /// <param name="availableWords">Word/hint pairs which are active in this game.</param>
        public void SetHintOptions(List<WordHintPair> availableWords)
        {
            foreach (WordHintPair word in availableWords)
            {
                if (word == null || string.IsNullOrEmpty(word.Word))
                    continue;

                HintOptions.Add(new HintOption(word.Word, word.Hint));
            }
        }

        /// <summary>
        /// Removes a hint option using the word the hint option object is representing.
        /// </summary>
        /// <param name="word">The word which the hint option represents.</param>
        public void RemoveHintOption(string word)
        {
            // Find the hint with the given word and remove it from the list.
            HintOption hint = HintOptions.Find(x => KurmanjiTextUtility.WordsEqual(x.Word, word));
            HintOptions.Remove(hint);
            if (CurrentRevealedDescriptiveHint != null && KurmanjiTextUtility.WordsEqual(CurrentRevealedDescriptiveHint.Word, word))
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
        public void AddFoundWord(string foundWord) => CorrectlyAddedWords.Add(KurmanjiTextUtility.NormalizeWord(foundWord));

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

            return KurmanjiTextUtility.ToDisplayUpper(sequence);
        }
    }
}

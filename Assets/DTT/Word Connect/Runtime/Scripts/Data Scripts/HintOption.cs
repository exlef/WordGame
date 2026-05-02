using System.Collections.Generic;
using System.Linq;

namespace DTT.WordConnect
{
    /// <summary>
    /// This class holds all necessary information for a single word's hint possibilities.
    /// </summary>
    public class HintOption
    {
        /// <summary>
        /// The word this hint option represents
        /// </summary>
        public string Word { get; private set; }

        /// <summary>
        /// A short description of the word used for the descriptive hint.
        /// </summary>
        public string WordDescription { get; private set; }

        /// <summary>
        /// List of bools which indicate whether an index (letter) has been revealed.
        /// </summary>
        public List<bool> LetterRevealed { get; private set; }

        /// <summary>
        /// Boolean whether this hintOption has already been used for a descriptive hint.
        /// </summary>
        public bool DescriptionRevealed { get; private set; }

        /// <summary>
        /// Constructor builds a new HintOption object from a word and a description.
        /// </summary>
        /// <param name="wordHintOption">The word this HintOption will represent.</param>
        /// <param name="descriptionHint">The description attached to the given word.</param>
        public HintOption(string wordHintOption, string descriptionHint)
        {
            Word = wordHintOption;
            WordDescription = descriptionHint;
            // Build a list of false bools with the word as length of the list.
            LetterRevealed = Enumerable.Repeat(false, wordHintOption.Length).ToList();
            // If the description is empty initialize the hint option as if its description has already been revealed.
            // Some developers might not want to implement descriptive hints.
            DescriptionRevealed = string.IsNullOrEmpty(descriptionHint);
        }

        /// <summary>
        /// Sets the description to revealed.
        /// </summary>
        public void SetDescriptionRevealed() => DescriptionRevealed = true;

        /// <summary>
        /// Sets all letters to revealed.
        /// </summary>
        public void SetWordRevealed()
        {
            for(int i = 0; i < LetterRevealed.Count; i++)
                SetLetterRevealed(i);
        }

        /// <summary>
        /// Sets a certain index of the word to revealed.
        /// </summary>
        /// <param name="index">The letter index of the word which should be revealed.</param>
        public void SetLetterRevealed(int index) => LetterRevealed[index] = true;
    }
}
namespace DTT.WordConnect
{
    /// <summary>
    /// Struct to represent a single letter's input.
    /// </summary>
    public struct LetterInput
    {
        /// <summary>
        /// The unique id of this input, necessary to distinguish between letter input with the same char.
        /// </summary>
        public readonly int id;

        /// <summary>
        /// The letter that belongs to this input.
        /// </summary>
        public readonly char letter;

        /// <summary>
        /// Creates a new LetterInput object with the specified id and letter.
        /// </summary>
        /// <param name="identifier">The identifier for this letter.</param>
        /// <param name="inputLetter">The char corresponding to this input.</param>
        public LetterInput(int identifier, char inputLetter)
        {
            id = identifier;
            letter = inputLetter;
        }
    }
}

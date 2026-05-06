using System;
using UnityEngine;

namespace DTT.WordConnect
{
    /// <summary>
    /// Pairs a word with its descriptive hint inside a level configuration.
    /// </summary>
    [Serializable]
    public class WordHintPair
    {
        /// <summary>
        /// The word associated with the paired hint.
        /// </summary>
        [SerializeField]
        private string word;

        /// <summary>
        /// The hint associated with the paired word.
        /// </summary>
        [SerializeField]
        private string hint;

        /// <summary>
        /// <inheritdoc cref="word"/>
        /// </summary>
        public string Word => KurmanjiTextUtility.NormalizeWord(word);

        /// <summary>
        /// <inheritdoc cref="hint"/>
        /// </summary>
        public string Hint => KurmanjiTextUtility.NormalizeDisplayText(hint);

        /// <summary>
        /// Creates an empty word/hint pair.
        /// </summary>
        public WordHintPair()
        {
            word = string.Empty;
            hint = string.Empty;
        }

        /// <summary>
        /// Creates a word/hint pair with normalized values.
        /// </summary>
        /// <param name="word">The word to store.</param>
        /// <param name="hint">The hint to store.</param>
        public WordHintPair(string word, string hint)
        {
            this.word = word;
            this.hint = hint;
            Normalize();
        }

        /// <summary>
        /// Normalizes the serialized values for storage.
        /// </summary>
        public void Normalize()
        {
            word = KurmanjiTextUtility.NormalizeWord(word);
            hint = KurmanjiTextUtility.NormalizeDisplayText(hint);
        }
    }
}

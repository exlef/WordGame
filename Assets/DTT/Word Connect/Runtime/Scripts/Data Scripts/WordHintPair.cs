using DTT.Utils.Extensions;
using UnityEngine;

    namespace DTT.WordConnect
{
    /// <summary>
    /// The purpose of this class is to pair a word with an associated hint.
    /// </summary>
    [CreateAssetMenu(fileName = "Word Hint Pair", menuName = "DTT/Word Connect/Word Hint Pair")]
    public class WordHintPair : ScriptableObject
    {
        /// <summary>
        /// The word associated with the paired hint.
        /// </summary>
        [field: SerializeField]
        public string Word { get; private set; }

        /// <summary>
        /// The hint associated with the paired word.
        /// </summary>
        [field: SerializeField]
        public string Hint { get; private set; }

        /// <summary>
        /// Automatically set the word to the name of the asset if none is given.
        /// </summary>
        private void OnValidate()
        {
            if (Word.IsNullOrEmpty())
                Word = this.name;

            Word = KurmanjiTextUtility.NormalizeWord(Word);
            Hint = KurmanjiTextUtility.NormalizeDisplayText(Hint);
        }
    }
}

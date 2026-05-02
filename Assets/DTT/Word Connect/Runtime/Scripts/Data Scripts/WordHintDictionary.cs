using DTT.Utils.Extensions;
using UnityEngine;

namespace DTT.WordConnect
{
    /// <summary>
    /// The purpose of this class is to hold an array of <see cref="WordHintPair"/> objects,
    /// so that a given dictionary may be used when generating a word layout.
    /// </summary>
    [CreateAssetMenu(fileName = "Word Hint Dictionary", menuName = "DTT/Word Connect/Word Hint Dictionary")]
    public class WordHintDictionary : ScriptableObject
    {
        /// <summary>
        /// The identifying name of the dictionary.
        /// </summary>
        [field: SerializeField]
        public string DictionaryName { get; private set; }

        /// <summary>
        /// The array of word hint pairs that are associated with this dictionary object.
        /// </summary>
        [field: SerializeField]
        public WordHintPair[] WordHintPairs { get; private set; }

        /// <summary>
        /// Automatically set the dictionary name to the name of the asset if none is given.
        /// </summary>
        private void OnValidate()
        {
            if (DictionaryName.IsNullOrEmpty())
                DictionaryName = this.name;
        }
    }
}


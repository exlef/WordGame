using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DTT.Utils.Extensions;

namespace DTT.WordConnect
{
    /// <summary>
    /// The purpose of this class is to hold a <see cref="WordHintDictionary"/> and associated
    /// Word Connect related data that represents it.
    /// </summary>
    [CreateAssetMenu(fileName = "Word Connect Data", menuName = "DTT/Word Connect/Config")]
    public class WordConnectConfigurationData : ScriptableObject
    {
        /// <summary>
        /// The dictionary that should be associated with a given game's puzzle data.
        /// </summary>
        [SerializeField]
        private string _gameName;

        /// <summary>
        /// <inheritdoc cref="_gameName"/>
        /// </summary>
        public string GameName => _gameName;

        /// <summary>
        /// The amount of word hints available in this game.
        /// </summary>
        [SerializeField]
        private int _wordHintsAvailable;

        /// <summary>
        /// <inheritdoc cref="_wordHintsAvailable"/>
        /// </summary>
        public int WordHintsAvailable => _wordHintsAvailable;

        /// <summary>
        /// The amount of letter hints available in this game.
        /// </summary>
        [SerializeField]
        private int _letterHintsAvailable;

        /// <summary>
        /// <inheritdoc cref="_letterHintsAvailable"/>
        /// </summary>
        public int LetterHintsAvailable => _letterHintsAvailable;

        /// <summary>
        /// The amount of description hints available in this game.
        /// </summary>
        [SerializeField]
        private int _descriptionHintsAvailable;

        /// <summary>
        /// <inheritdoc cref="_descriptionHintsAvailable"/>
        /// </summary>
        public int DescriptionHintsAvailable => _descriptionHintsAvailable;

        /// <summary>
        /// The dictionary that should be associated with a given WordConnect puzzles data.
        /// </summary>
        [SerializeField]
        private WordHintDictionary _dictionaryAsset;

        /// <summary>
        /// <inheritdoc cref="_dictionaryAsset"/>
        /// </summary>
        public WordHintDictionary DictionaryAsset => _dictionaryAsset;

        /// <summary>
        /// If true, using a word hint will try to reveal the longest word in the word grid.
        /// </summary>
        [SerializeField]
        private bool _prioritizeLongestWordHint = false;

        /// <summary>
        /// <inheritdoc cref="_prioritizeLongestWordHint"/>
        /// </summary>
        public bool PrioritizeLongestWordHint => _prioritizeLongestWordHint;

        /// <summary>
        /// The layout associated with the game.
        /// </summary>
        [SerializeField]
        private List<WordVector> _wordVectors;

        /// <summary>
        /// <inheritdoc cref="_wordVectors"/>
        /// </summary>
        public List<WordVector> WordVectors => _wordVectors;

        /// <summary>
        /// Two dimensional array that represents a grid of letters.
        /// The purpose of this is to create quick previews of game layouts without logic.
        /// </summary>
        private char[,] _crosswordLetters;

        /// <summary>
        /// <inheritdoc cref="_crosswordLetters"/>
        /// </summary>
        public char[,] CrosswordLetters => _crosswordLetters;

        /// <summary>
        /// Int to keep track of the grid width.
        /// Used exclusively for the custom editor.
        /// </summary>
        [SerializeField]
        private int _gridWidth;

        /// <summary>
        /// 1D array representation of the 2D crosswordLetters array.
        /// Used exclusively for the custom editor.
        /// </summary>
        [SerializeField]
        private string[] _flattenedLetterLayout;

        /// <summary>
        /// Builds a partial game layout from the saved data. 
        /// The word vectors are returned as copies so that they may correctly be constructed.
        /// </summary>
        /// <returns>A partially constructed game layout.</returns>
        public WordConnectLayout ReturnLayout() => new WordConnectLayout(null, _crosswordLetters, _wordVectors.Select(x => x.ReturnCopy()).ToList());

        /// <summary>
        /// Sets the list of word vectors.
        /// </summary>
        /// <param name="wordVectors">The words being applied to the game data.</param>
        public void SetWordVectors(List<WordVector> wordVectors) => _wordVectors = wordVectors;

        /// <summary>
        /// Sets the grid of letters.
        /// </summary>
        /// <param name="crosswordLetters">The grid of letters representing the current game.</param>
        public void SetCrosswordLetters(char[,] crosswordLetters)
        {
            // Set the crosswordLetters 2D array.
            _crosswordLetters = crosswordLetters;
            _gridWidth = _crosswordLetters.GetLength(0);

            // Flatten the crosswordLetters to a 1D array.
            _flattenedLetterLayout = new string[_crosswordLetters.GetLength(0) * _crosswordLetters.GetLength(1)];
            int iteration = 0;
            for (int x = crosswordLetters.GetLength(1) - 1; x >= 0; x--)
            {
                for (int y = 0; y < crosswordLetters.GetLength(0); y++)
                {
                    _flattenedLetterLayout[iteration] = crosswordLetters[y, x].ToString();
                    iteration++;
                }
            }
        }

        /// <summary>
        /// Checks whether a certain letter of a word overlaps in the game layout with an other word.
        /// </summary>
        /// <param name="word">the word to check for overlap.</param>
        /// <param name="letterIndex">the letter's index of the word given word.</param>
        /// <returns>Boolean whether the letter overlaps with a different word or not.</returns>
        public bool LetterHasOverlap(string word, int letterIndex)
        {
            // Get the WordVector object that represents the given word variable.
            WordVector wordVectorToCheck = _wordVectors.Find(wordVector => KurmanjiTextUtility.WordsEqual(wordVector.WordHintPair.Word, word));

            // Get the position the given letterIndex occupies.
            Vector2Int letterPosition = wordVectorToCheck.OccupiedPositions[letterIndex];

            foreach (WordVector wordVector in WordVectors)
            {
                // Skip the wordVector we are checking for.
                if (KurmanjiTextUtility.WordsEqual(wordVector.WordHintPair.Word, wordVectorToCheck.WordHintPair.Word)) continue;
                // If any other words occupy the same position there is overlap.
                if (wordVector.OccupiedPositions.Contains(letterPosition)) return true;

            }
            return false;
        }
        /// <summary>
        /// Gets the word and letter index of the word that overlaps with the given word and letter index.
        /// </summary>
        /// <param name="word">the word to check for overlap.</param>
        /// <param name="letterIndex">the letter's index of the word given word.</param>
        /// <returns>String and int Tuple respectively representing the word and the index of the letter that overlaps.</returns>
        public (string, int) GetLetterOverlap(string word, int letterIndex)
        {
            // Get the WordVector object that represents the given word variable.
            WordVector wordVectorToCheck = _wordVectors.Find(wordVector => KurmanjiTextUtility.WordsEqual(wordVector.WordHintPair.Word, word));
            // Calculate all positions the WordVector occupies in the grid.
            _wordVectors.ForEach(wordVector => wordVector.CalculatePositions());

            // Get the position the given letterIndex occupies.
            Vector2Int letterPosition = wordVectorToCheck.OccupiedPositions[letterIndex];
            foreach (WordVector wordVector in WordVectors)
            {
                // Skip the wordVector we are checking for.
                if (KurmanjiTextUtility.WordsEqual(wordVector.WordHintPair.Word, wordVectorToCheck.WordHintPair.Word))
                    continue;

                // If any other words occupy the same position there is overlap.
                // Return the overlapping word and its index.
                if (wordVector.OccupiedPositions.Contains(letterPosition))
                    return (wordVector.WordHintPair.Word, wordVector.OccupiedPositions.IndexOf(letterPosition));

            }
            return (null, 0);
        }

        /// <summary>
        /// Automatically set the game's name to the name of the asset if none is given.
        /// </summary>
        private void OnValidate()
        {
            if (GameName.IsNullOrEmpty())
                _gameName = this.name;
        }
    }
}



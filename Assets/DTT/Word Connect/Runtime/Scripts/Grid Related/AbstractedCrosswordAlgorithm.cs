using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DTT.WordConnect
{
    /// <summary>
    /// Class that manages the generation and creation of crossword layouts.
    /// </summary>
    public static class AbstractedCrosswordAlgorithm
    {
        /// <summary>
        /// Variable that decides on the likelihood that the first word placed is down.
        /// The higher this value, the more likely it will be placed downward first.
        /// </summary>
        private static readonly float _firstWordDownWeight = 0.5f;

        /// <summary>
        /// How many times the failed attempt list should be iterated over before the algorithm gives up.
        /// </summary>
        private static readonly float _failedAttemptLimit = 10;

        /// <summary>
        /// Generates a two dimensional character array that represents a crossword layout.
        /// </summary>
        /// <param name="wordHintDictionary">The dictionary being used in creation.</param>
        /// <param name="debugAlgorithm">If the algorithm should debug the results of generation.</param>
        /// <returns>A list of word vectors representing a crossword.</returns>
        public static List<WordVector> GetVectorCrosswordLayout(WordHintDictionary wordHintDictionary, bool debugAlgorithm)
        {
            // Temporary dictionary to hold currently used and unused words and the number of times it has failed to be used.
            Dictionary<WordHintPair, int> unusedWords = wordHintDictionary.WordHintPairs.ToDictionary(x => x, x => 0);

            // Track words in vector formats for crossword building.
            List<WordVector> wordVectors = new List<WordVector>();

            // Start off the grid with one random word.
            WordHintPair randomWord = unusedWords.Keys.ToArray()[Random.Range(0, unusedWords.Keys.Count)];
            unusedWords.Remove(randomWord);
            wordVectors.Add(new WordVector(randomWord, Vector2Int.zero, Random.Range(0f, 1) <= _firstWordDownWeight));

            // Helper int to track lowest fail count and failed attempts.
            int lowestFailCount = 0;

            // Helper list to track words with the lowest fail count.
            List<WordHintPair> lowestFailWords = new List<WordHintPair>();

            // Helper reference for new possible word vectors.
            WordVector newWordVector;

            // Attempt to iterate over all unused words and fit them to a grid.
            while (unusedWords.Count > 0 && lowestFailCount < _failedAttemptLimit)
            {
                // Get list of words with the lowest fail count.
                lowestFailWords.Clear();
                foreach (KeyValuePair<WordHintPair, int> wordPair in unusedWords)
                    if (wordPair.Value <= lowestFailCount)
                        lowestFailWords.Add(wordPair.Key);

                // Check that the list was populated, otherwise try again with a higher fail bound.
                if (lowestFailWords.Count == 0)
                {
                    lowestFailCount++;
                    continue;
                }

                // Get a random word and try to fit it to the current layout.
                randomWord = lowestFailWords[Random.Range(0, lowestFailWords.Count)];

                // If sucessful, remove, otherwise incrememnt its tracked failures.
                newWordVector = GetValidWordVector(randomWord, wordVectors);
                if (newWordVector.WordHintPair)
                {
                    unusedWords.Remove(randomWord);
                    wordVectors.Add(newWordVector);
                }
                else
                {
                    unusedWords[randomWord]++;
                }
            }

            // Debugging toggle for words used and excluded.
            if (debugAlgorithm)
            {
                Debug.Log("Order of words:");
                for (int i = 0; i < wordVectors.Count; i++)
                {
                    WordVector item = wordVectors[i];
                    Debug.Log(string.Concat(i, ". ", item.WordHintPair.Word));
                }

                Debug.Log(string.Concat("Excluded " + unusedWords.Count, " words after ", lowestFailCount, " failure cycles:"));
                foreach (KeyValuePair<WordHintPair, int> v in unusedWords)
                    Debug.Log(string.Concat(v.Key.Word));
            }

            // Return the word vectors.
            return wordVectors;
        }

        /// <summary>
        /// Gets a valid crossword word vector for a given vector word grid.
        /// </summary>
        /// <param name="givenPair">The new pair to be added to a vector grid.</param>
        /// <param name="currentVectorGrid">The current vector grid that is in place.</param>
        /// <returns>A valid word vector that fits in the grid, otherwise an empty word vector struct.</returns>
        public static WordVector GetValidWordVector(WordHintPair givenPair, List<WordVector> currentVectorGrid)
        {
            // Find all potential placements.
            foreach (WordVector wordVector in currentVectorGrid)
            {
                // Check for valid overlaps in this word.
                for (int i = 0; i < wordVector.WordHintPair.Word.Length; i++)
                {
                    char checkedLetter = wordVector.WordHintPair.Word[i];

                    // Check if any of the letters can be extended from.
                    for (int i1 = 0; i1 < givenPair.Word.Length; i1++)
                    {
                        if (char.ToUpper(checkedLetter) == char.ToUpper(givenPair.Word[i1]))
                        {
                            // Helper bool to track if the new word is extending down or across from the checked letter.
                            bool isDown = !wordVector.IsDown;

                            // Helper vector that will be used in testing potential origin points.
                            Vector2Int simulatedOrigin = wordVector.OccupiedPositions[i] + new Vector2Int(isDown ? 0 : -i1, isDown ? i1 : 0);

                            // Temporary word vector declaration for overlap testing.
                            WordVector potentialWordVector = new WordVector(givenPair, simulatedOrigin, isDown);

                            // Helper bool for tracking if an overlap was detected.
                            bool hadOverlap = false;

                            // Check all other word vectors, excluding the currently interacted one, for potential and unintended overlaps.
                            foreach (WordVector deepWordVector in currentVectorGrid)
                            {
                                if (deepWordVector.CheckForOverlap(potentialWordVector) && !wordVector.Equals(deepWordVector))
                                {
                                    hadOverlap = true;
                                    break;
                                }
                            }

                            // Return if a valid word vector was found.
                            if (!hadOverlap)
                                return potentialWordVector;
                        }
                    }
                }
            }

            return new WordVector();
        }

        /// <summary>
        /// Generates an empty two dimensional array grid.
        /// </summary>
        /// <param name="dimensions">The width and height to generate a layout with.</param>
        /// <returns>An empty two dimensional character array grid.</returns>
        public static char[,] GetEmptyCharacterLayout(Vector2Int dimensions)
        {
            char[,] temporaryGrid = new char[dimensions.x, dimensions.y];

            for (int x = 0; x < dimensions.x; x++)
                for (int y = 0; y < dimensions.y; y++)
                    temporaryGrid[x, y] = '\0';

            return temporaryGrid;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace DTT.WordConnect
{
    /// <summary>
    /// Data struct that is meant to handle an abstract interpretation of a word and its placement on a grid.
    /// </summary>
    [Serializable]
    public struct WordVector
    {
        /// <summary>
        /// The origin of the word in an abstracted grid, representing columns and rows.
        /// </summary>
        [field: SerializeField]
        public Vector2Int OriginPosition { get; private set; }

        /// <summary>
        /// The orientation of the word and if it is down or across.
        /// </summary>
        [field: SerializeField]
        public bool IsDown { get; private set; }

        /// <summary>
        /// The positions that are occupied by the letters in the word.
        /// </summary>
        [field: SerializeField]
        public List<Vector2Int> OccupiedPositions { get; private set; }

        /// <summary>
        /// The positions that this word could potentially have conflicts with other words.
        /// </summary>
        [field: SerializeField]
        public List<Vector2Int> InfluencedPositions { get; private set; }

        /// <summary>
        /// The word associated with this vector, important in calculating
        /// occupied or influenced positions and the creation of a word tile.
        /// </summary>
        [field: SerializeField]
        public string Word { get; private set; }

        /// <summary>
        /// Constructor for quick creation of word vector.
        /// </summary>
        /// <param name="word">The word that this vector represents.</param>
        /// <param name="originPosition">The origin for this word in an abstract grid.</param>
        /// <param name="isDown">If the words orientation is down or across.</param>
        public WordVector(string word, Vector2Int originPosition, bool isDown)
        {
            // Setup parameter data.
            Word = KurmanjiTextUtility.NormalizeWord(word);
            OriginPosition = originPosition;
            IsDown = isDown;

            OccupiedPositions = new List<Vector2Int>();
            InfluencedPositions = new List<Vector2Int>();

            // Setup position relevant data.
            CalculatePositions();
        }

        /// <summary>
        /// Creates a copy of the word vector.
        /// </summary>
        /// <returns>A copy of the word vector.</returns>
        public WordVector ReturnCopy() => new WordVector(Word, OriginPosition, IsDown);

        /// <summary>
        /// Single method that calls both position calculation methods.
        /// </summary>
        public void CalculatePositions()
        {
            CalculateOccupiedPositions();
            CalculateInfluencedPositions();
        }

        /// <summary>
        /// Calculates and sets all occupied positions based on the word vectors orientation and length.
        /// </summary>
        private void CalculateOccupiedPositions()
        {
            // Clear the list of old positions.
            if (OccupiedPositions == null) OccupiedPositions = new List<Vector2Int>();
            if (OccupiedPositions.Count > 0) OccupiedPositions.Clear();

            // Add positions based on the length and orientation of the word.
            for (int i = 0; i < Word.Length; i++)
            {
                Vector2Int newPosition = OriginPosition + new Vector2Int(IsDown ? 0 : i, IsDown ? -i : 0);
                OccupiedPositions.Add(newPosition);
            }

        }

        /// <summary>
        /// Calculates and sets all influenced positions based on the word vectors occupied positions.
        /// </summary>
        private void CalculateInfluencedPositions()
        {
            // Clear the list of old positions.
            InfluencedPositions.Clear();

            // Add positions based on the length and orientation of the word.
            for (int i = 0; i < OccupiedPositions.Count; i++)
            {
                Vector2Int origin = OccupiedPositions[i];

                // Track relevant positions.
                if (IsDown)
                {
                    InfluencedPositions.Add(origin + Vector2Int.left);
                    InfluencedPositions.Add(origin + Vector2Int.right);

                    if (i == 0)
                        InfluencedPositions.Add(origin + Vector2Int.up);
                    else if (i == OccupiedPositions.Count - 1)
                        InfluencedPositions.Add(origin + Vector2Int.down);
                }
                else
                {
                    InfluencedPositions.Add(origin + Vector2Int.up);
                    InfluencedPositions.Add(origin + Vector2Int.down);

                    if (i == 0)
                        InfluencedPositions.Add(origin + Vector2Int.left);
                    else if (i == OccupiedPositions.Count - 1)
                        InfluencedPositions.Add(origin + Vector2Int.right);
                }
            }
        }

        /// <summary>
        /// Checks for overlap between an origin and a given word vectors influenced or occupied positions.
        /// </summary>
        /// <param name="givenWordVector">The word vector being compared against.</param>
        /// <returns>If overlaps exists.</returns>
        public bool CheckForOverlap(WordVector givenWordVector)
        {
            foreach (Vector2Int item in InfluencedPositions)
                if (givenWordVector.OccupiedPositions.Contains(item))
                    return true;

            foreach (Vector2Int item in OccupiedPositions)
                if (givenWordVector.OccupiedPositions.Contains(item))
                    return true;

            return false;
        }
    }
}

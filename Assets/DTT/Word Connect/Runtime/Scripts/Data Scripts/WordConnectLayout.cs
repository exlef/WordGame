using System;
using System.Collections.Generic;
using UnityEngine;

namespace DTT.WordConnect
{
    /// <summary>
    /// The purpose of this class is to manage data relating to a word layout.
    /// </summary>
    [Serializable]
    public struct WordConnectLayout
    {
        /// <summary>
        /// Two dimensional array that will represent the grid of word tiles.
        /// </summary>
        public GameObject[,] GridTiles {get; private set;}

        /// <summary>
        /// Two dimensional array that will represent the grid of letters.
        /// </summary>
        public char[,] GridLetters { get; private set; }

        /// <summary>
        /// Game words represented in vector format.
        /// </summary>
        public List<WordVector> WordVectors { get; private set; }

        /// <summary>
        /// Sets the 2D array of Gameobjects representing the game grid.
        /// </summary>
        /// <param name="gridTiles">2D array of GameObjects representing the game grid.</param>
        public void SetGridTiles(GameObject[,] gridTiles) => GridTiles = gridTiles;

        /// <summary>
        /// Sets the 2D array of chars representing the grid of letters.
        /// </summary>
        /// <param name="gridLetters">2D array of chars representing the grid of letters.</param>
        public void SetGridLetters(char[,] gridLetters) => GridLetters = gridLetters;

        /// <summary>
        /// Sets the WordVectors in this game layout.
        /// </summary>
        /// <param name="wordVectors">The list of WordVectors in this layout.</param>
        public void SetWordVectors(List<WordVector> wordVectors) => WordVectors = wordVectors;

        /// <summary>
        /// Constructor for quick creation of the struct.
        /// </summary>
        /// <param name="gridTiles">The list of GameObjects currently representing the layout.</param>
        /// <param name="gridLetters">The two dimensional array of letters representing the layout.</param>
        /// <param name="wordVectors">The list of word vectors currently representing the layout.</param>
        public WordConnectLayout(GameObject[,] gridTiles, char[,] gridLetters, List<WordVector> wordVectors)
        {
            GridTiles = gridTiles;
            GridLetters = gridLetters;
            WordVectors = wordVectors;
        }

        /// <summary>
        /// Update the layout with the values of another.
        /// </summary>
        public void UpdateLayout(WordConnectLayout layout)
        {
            GridTiles = layout.GridTiles;
            GridLetters = layout.GridLetters;
            WordVectors = layout.WordVectors;
        }
    }
}
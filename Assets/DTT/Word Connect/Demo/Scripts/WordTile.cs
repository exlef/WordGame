using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Class that represents and manages a collection of <see cref="LetterTile"/> and their associated letter.
    /// </summary>
    public class WordTile : ScriptableObject
    {
        /// <summary>
        /// Reference to the grid manager script to fetch animation variables.
        /// </summary>
        private WordConnectGridManager _gridManager;

        /// <summary>
        /// Booleans to keep track if this word has been found.
        /// </summary>
        private bool _isFound;

        /// <summary>
        /// The word hint pair that this class will attempt to represent and manage.
        /// </summary>
        public WordHintPair WordHint { get; private set; }

        /// <summary>
        /// List of letter tiles that are associated with this specific word.
        /// </summary>
        private List<LetterTile> _letterTiles;

        /// <summary>
        /// <inheritdoc cref="_letterTiles"/>
        /// </summary>
        public List<LetterTile> LetterTiles => _letterTiles;

        /// <summary>
        /// The letter tile that has an overlap and its associated <see cref="WordTile"/>.
        /// </summary>
        private Dictionary<LetterTile, WordTile> _overlappingElements;

        /// <summary>
        /// Creates a new Scriptable Object Instance of <see cref="WordTile"/> and initializes its variables.
        /// </summary>
        /// <param name="wordHintPair">The word this returned WordTile will represent.</param>
        /// <returns>A new <see cref="WordTile"/> representing the given WordHintPair.</returns>
        public static WordTile CreateWordTile(WordHintPair wordHintPair)
        {
            WordTile wordTile = CreateInstance<WordTile>();
            wordTile.WordHint = wordHintPair;
            wordTile._isFound = false;
            wordTile._letterTiles = new List<LetterTile>();
            wordTile._overlappingElements = new Dictionary<LetterTile, WordTile>();

            return wordTile;
        }

        /// <summary>
        /// Sets all setup variables necessary.
        /// </summary>
        /// <param name="gridManager">Reference to the manager to fetch the animation variables.</param>
        public void SetupTile(WordConnectGridManager gridManager) => _gridManager = gridManager;

        /// <summary>
        /// This functions listens to the GameState event invoked when the game state updates.
        /// Update all tiles this word represents to refelect the game state change.
        /// </summary>
        /// <param name="state">Reference to the game state class.</param>
        public void GameStateUpdated(WordConnectState state)
        {
            if (state.CurrentWordInput.Count > 0) return;
            CheckIfWordFound(state);
            CheckIfLetterHinted(state);
        }

        /// <summary>
        /// Checks whether one or more letters of this word have been highlighted.
        /// </summary>
        /// <param name="state">The game updated game state.</param>
        private void CheckIfLetterHinted(WordConnectState state)
        {
            // If the CompletelyHintedWords list contains this word, set all letters to hinted.
            if (state.CompletelyHintedWords.Any(x => KurmanjiTextUtility.WordsEqual(x.Word, WordHint.Word)))
            {
                SetHintedLetters();
                return;
            }

            foreach (HintOption hint in state.HintOptions)
            {
                // If this HintOption represents the same word, update the letter tiles state to reflect which letters are revealed.
                if (KurmanjiTextUtility.WordsEqual(hint.Word, WordHint.Word))
                {
                    SetHintedLetters(hint.LetterRevealed);
                    return;
                }
            }
        }

        /// <summary>
        /// Sets this words letter tiles to the given state.
        /// </summary>
        /// <param name="lettersHinted">List of booleans which represent per index what letter has been revealed.</param>
        private void SetHintedLetters(List<bool> lettersHinted)
        {
            for (int i = 0; i < lettersHinted.Count; i++)
            {
                // If the letter is revealed and hasn't already been revealed to prevent reanimating.
                if (!_letterTiles[i].IsHinted && lettersHinted[i])
                {
                    // Update the state, color and scale of the revealed tile.
                    _letterTiles[i].IsHinted = true;
                    if (_letterTiles[i].gameObject.activeInHierarchy)
                    {
                        _letterTiles[i].AnimateColors(_gridManager.PerWordAnimationDuration / _letterTiles.Count);
                        _letterTiles[i].AnimateLetterCompletion(_gridManager.PerWordAnimationDuration, 0f);
                    }
                    else
                    {
                        _letterTiles[i].UpdateColors();
                    }
                }
            }
        }

        /// <summary>
        /// Sets all letter tiles of this word to revealed.
        /// </summary>
        private void SetHintedLetters()
        {
            for (int i = 0; i < _letterTiles.Count; i++)
            {
                // Check whether the letter hasn't already been revealed to prevent reanimating.
                if (!_letterTiles[i].IsHinted)
                {
                    // Update the state, color and scale of the revealed tile.
                    _letterTiles[i].IsHinted = true;
                    if(_letterTiles[i].gameObject.activeInHierarchy)
                    {
                        _letterTiles[i].AnimateColors(_gridManager.PerWordAnimationDuration / _letterTiles.Count);
                        _letterTiles[i].AnimateLetterCompletion(_gridManager.PerWordAnimationDuration, 0f);
                    }
                    else
                    {
                        _letterTiles[i].UpdateColors();
                    }
                }
            }
        }

        /// <summary>
        /// Checks the game state if this word has been found. 
        /// </summary>
        /// <param name="state">The updated game state.</param>
        private void CheckIfWordFound(WordConnectState state)
        {
            if (state.CorrectlyAddedWords.Any(word => KurmanjiTextUtility.WordsEqual(word, WordHint.Word)) && !_isFound)
            {
                // Set state and update all letter tiles to visualize the word being found.
                _isFound = true;
                SetWordFound();
            }
        }

        /// <summary>
        /// Animates all letters to reflect them being found.
        /// </summary>
        private void SetWordFound()
        {
            foreach (LetterTile letterTile in _letterTiles)
            {
                letterTile.IsFound = true;
                // Animate colors and update letter's state.
                if (letterTile.gameObject.activeInHierarchy)
                    letterTile.AnimateColors(_gridManager.PerWordAnimationDuration);
                else
                    letterTile.UpdateColors();
            }
            bool allLetterTilesActive = _letterTiles.All(tile => tile.gameObject.activeInHierarchy);
            // Animate the scale of the letter tiles.
            if(allLetterTilesActive)
                AnimateWordCompletion(_gridManager.PerWordAnimationDuration, _gridManager.PerLetterAnimationDelay);
        }

        /// <summary>
        /// Add a letter to the word tile for tracking.
        /// </summary>
        /// <param name="letterTile">The letter tile to save.</param>
        public void AddLetter(LetterTile letterTile) => _letterTiles.Add(letterTile);

        /// <summary>
        /// Track the overlapping word and letter tile for interactivity flow logic.
        /// </summary>
        /// <param name="letterTile">The overlapping letter tile to track.</param>
        /// <param name="wordTile">The overlapping word tile to track.</param>
        public void ConnectOverlappingWordTile(LetterTile letterTile, WordTile wordTile) => _overlappingElements.Add(letterTile, wordTile);

        /// <summary>
        /// Checks if a given letter tile is contained in the word tile, allowing for overlap checks.
        /// </summary>
        /// <param name="letterTile">The letter tile being checked.</param>
        /// <returns>If the given letter tile is contained in the word tile.</returns>
        public bool ContainsLetterTile(LetterTile letterTile) => _letterTiles.Contains(letterTile);

        /// <summary>
        /// Animates the letter tiles scale asynchronous.
        /// </summary>
        /// <param name="animationDuration">Duration of the animation in seconds.</param>
        /// <param name="perLetterAnimationDelay">Delay before the animation starts in seconds.</param>
        /// <param name="tileScale">The scale the tiles will animate to and from.</param>
        private void AnimateWordCompletion(float animationDuration, float perLetterAnimationDelay)
        {
            int letterCount = _letterTiles.Count;
            float letterAnimationDuration = animationDuration / letterCount;

            for (int i = 0; i < _letterTiles.Count; i++)
            {
                float animationDelay = perLetterAnimationDelay * i;
                _letterTiles[i].AnimateLetterCompletion(letterAnimationDuration, animationDelay);
            }
        }
    }
}

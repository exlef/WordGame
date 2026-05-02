using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Script that handles the logic behind generating and managing grid elements.
    /// </summary>
    [RequireComponent(typeof(WordConnectManager))]
    public class WordConnectGridManager : MonoBehaviour
    {
        /// <summary>
        /// Reference to the manager script so that the layout can be built once the game starts.
        /// </summary>
        private WordConnectManager _wordConnectManager;

        /// <summary>
        /// The prefab that will be used when generating letter tiles.
        /// </summary>
        [SerializeField]
        private GameObject _letterTile;

        /// <summary>
        /// The prefab that will be used when generating non-letter tiles.
        /// </summary>
        [SerializeField]
        private GameObject _blockedTile;

        /// <summary>
        /// The current active level data.
        /// Used to generate new layouts and save them.
        /// </summary>
        [SerializeField]
        private WordConnectConfigurationData _gameData;

        /// <summary>
        /// The layout group that will be used to automatically format the grid.
        /// </summary>
        [SerializeField]
        private GridLayoutGroup _gridLayout;

        /// <summary>
        /// <inheritdoc cref="_gridLayout"/>
        /// </summary>
        public GridLayoutGroup GridLayout => _gridLayout;

        /// <summary>
        /// The preview layout group that will be used to automatically format the preview grid.
        /// </summary>
        [SerializeField]
        private GridLayoutGroup _previewGridLayout;

        /// <summary>
        /// The active game layout being used.
        /// </summary>
        private WordConnectLayout _gameLayout;

        /// <summary>
        /// The word tiles that manage letter tile interaction.
        /// </summary>
        private List<WordTile> _wordTiles;

        /// <summary>
        /// <inheritdoc cref="_wordTiles"/>
        /// </summary>
        public List<WordTile> WordTiles => _wordTiles;

        /// <summary>
        /// The letter tiles that represent the letters of a word in the game grid.
        /// </summary>
        private List<LetterTile> _letterTiles;

        /// <summary>
        /// The length of time at which a word completion animation is played.
        /// </summary>
        [SerializeField]
        private float _perWordAnimationDuration = 0.5f;

        /// <summary>
        /// <inheritdoc cref="_perWordAnimationDuration"/>
        /// </summary>
        public float PerWordAnimationDuration => _perWordAnimationDuration;

        /// <summary>
        /// The delay between each letter animation when animating a word.
        /// </summary>
        [SerializeField]
        private float _perLetterAnimationDelay = 0.25f;

        /// <summary>
        /// <inheritdoc cref="_perLetterAnimationDelay"/>
        /// </summary>
        public float PerLetterAnimationDelay => _perLetterAnimationDelay;

        /// <summary>
        /// The desired scale of the animated tiles.
        /// </summary>
        [SerializeField]
        private float _animationTileScale = 1.05f;

        /// <summary>
        /// <inheritdoc cref="_animationTileScale"/>
        /// </summary>
        public float AnimationTileScale => _animationTileScale;

        /// <summary>
        /// Defines if the crossword generation algorithm should be debugged.
        /// </summary>
        [SerializeField]
        private bool _debugAlgorithm;

        /// <summary>
        /// Generates one possible game layout and save it for displaying.
        /// </summary>
        public void GenerateNewVectorLayout() => _gameLayout.SetWordVectors(AbstractedCrosswordAlgorithm.GetVectorCrosswordLayout(_gameData.DictionaryAsset, _debugAlgorithm));

        /// <summary>
        /// Changes the active game configuration.
        /// </summary>
        /// <param name="givenConfiguration">The configuration data to change to.</param>
        public void ChangeGameConfiguration(WordConnectConfigurationData givenConfiguration) => _gameData = givenConfiguration;

        /// <summary>
        /// Loads the current/active layout from the game data object.
        /// </summary>
        public void LoadLayoutFromGameData() => _gameLayout.UpdateLayout(_gameData.ReturnLayout());

        /// <summary>
        /// Saves the current/active layout to the game data object for future use.
        /// </summary>
        public void SaveLayoutToGameData()
        {
            _gameData.SetWordVectors(_gameLayout.WordVectors);
            _gameData.SetCrosswordLetters(_gameLayout.GridLetters);

#if UNITY_EDITOR
            EditorUtility.SetDirty(_gameData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
      
        /// <summary>
        /// On awake initialize layout variable.
        /// </summary>
        private void Awake() => _gameLayout = new WordConnectLayout();

        /// <summary>
        /// Gets the reference to the manager script and starts listening to a initialized signal.
        /// </summary>
        private void OnEnable()
        {
            _wordConnectManager = WordConnectManager.Instance;
            _wordConnectManager.BuildGame += StartGame;
        }

        /// <summary>
        /// Stop listening to a initialized signal. 
        /// </summary>
        private void OnDisable() => _wordConnectManager.BuildGame -= StartGame;

        /// <summary>
        /// Is called when the manager scripts starts the game. 
        /// Builds the currently active game layout.
        /// </summary>
        private void StartGame()
        {
            ChangeGameConfiguration(_wordConnectManager.Configuration);
            LoadLayoutFromGameData();
            BuildActiveLayouts();
        }

        /// <summary>
        /// Properly disposes of old word tiles before resetting the list.
        /// </summary>
        /// <param name="wordTiles">The list of word tiles to be disposed of.</param>
        /// <param name="nullReset">If the list should be nullified or simply cleared.</param>
        public void DisposeWordTiles(List<WordTile> wordTiles, bool nullReset)
        {
            if (wordTiles == null)
                return;

            // Destroy the word tiles.
            for (int i = 0; i < wordTiles.Count; i++)
            {
                if (Application.isPlaying)
                {
                    _wordConnectManager.StateUpdated -= wordTiles[i].GameStateUpdated;
                    Destroy(wordTiles[i]);
                }
                else
                {
                    DestroyImmediate(wordTiles[i]);
                }
            }

            // Clean the list based on the given bool.
            if (nullReset)
                wordTiles = null;
            else
                wordTiles.Clear();
        }

        /// <summary>
        /// Properly disposes of old letter tiles before resetting the list.
        /// </summary>
        /// <param name="letterTiles">The list of letter tiles to be disposed of.</param>
        /// <param name="nullReset">If the list should be nullified or simply cleared.</param>
        public void DisposeLetterTiles(List<LetterTile> letterTiles, bool nullReset)
        {
            if (letterTiles == null)
                return;

            // Destroy the letter tiles.
            for (int i = 0; i < _letterTiles.Count; i++)
            {
                if (Application.isPlaying)
                    Destroy(letterTiles[i]);
                else
                    DestroyImmediate(letterTiles[i]);
            }
            
            // Clean the list based on the given bool.
            if (nullReset)
                letterTiles = null;
            else
                letterTiles.Clear();
        }

        /// <summary>
        /// Builds the active and preview layouts and sort the relevant listeners.
        /// </summary>
        public void BuildActiveLayouts()
        {
            // Enable/Disable relevant layout groups.
            _gridLayout.gameObject.SetActive(Application.isPlaying);
            _previewGridLayout.gameObject.SetActive(!Application.isPlaying);

            DisposeWordTiles(_wordTiles, true);
            DisposeLetterTiles(_letterTiles, true);

            // Primary layout setup.
            SetupLayoutSettings(_gridLayout);
            SetupLayoutSettings(_previewGridLayout);

            // Build the layouts.
            BuildLayout(_gridLayout, out _wordTiles, out _letterTiles);
            BuildLayoutPreview();
        }

        /// <summary>
        /// Correctly sets the given grid layout to match the generation algorithm.
        /// </summary>
        public void SetupLayoutSettings(GridLayoutGroup layoutGroup)
        {
            // Setup the grids.
            layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
            layoutGroup.childAlignment = TextAnchor.LowerLeft;
        }

        /// <summary>
        /// Builds the currently active game layout to the given parent layout group.
        /// </summary>
        /// <param name="parent">The Grid Layout Group in which the tiles will be generated.</param>
        /// <param name="wordTiles">List of the generated word tiles.</param>
        /// <param name="letterTiles">List of the generated letter tiles.</param>
        public void BuildLayout(GridLayoutGroup parent, out List<WordTile> wordTiles, out List<LetterTile> letterTiles)
        {
            // Reset the tracking lists.
            wordTiles = new List<WordTile>();
            letterTiles = new List<LetterTile>();

            // Remove old child/grid objects.
            for (int i = parent.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(parent.transform.GetChild(i).gameObject);

            // Calculate the bounds of the vectors for advanced usage.
            int xUpperBound = _gameLayout.WordVectors.Max(x => x.OccupiedPositions.Last().x);
            int xLowerBound = _gameLayout.WordVectors.Min(x => x.OccupiedPositions.First().x);
            int yUpperBound = _gameLayout.WordVectors.Max(y => y.OccupiedPositions.First().y);
            int yLowerBound = _gameLayout.WordVectors.Min(y => y.OccupiedPositions.Last().y);

            // The offset that should be used.
            Vector2Int vectorOffset = Vector2Int.zero;

            // Add the bound if it is negative.
            vectorOffset.x -= (xLowerBound < 0) ? xLowerBound : 0;
            vectorOffset.y -= (yLowerBound < 0) ? yLowerBound : 0;

            // Calculate the dimensions.
            Vector2Int dimensions = Vector2Int.one;
            dimensions.x += xUpperBound + Mathf.Abs(xLowerBound);
            dimensions.y += yUpperBound + Mathf.Abs(yLowerBound);

            // Set the grid contstraints to be correct.
            parent.constraintCount = dimensions.x;

            // Declare grid dimensions.
            GameObject[,] temporaryGrid = new GameObject[dimensions.x, dimensions.y];

            // Declare the preview grid dimensions.
            char[,] temporaryPreviewGrid = AbstractedCrosswordAlgorithm.GetEmptyCharacterLayout(dimensions);

            // Iterate over each vector pair and create the tile on the grid.
            foreach (WordVector wordVector in _gameLayout.WordVectors)
            {
                // Declare and instance a new word tile to manage individual tiles.
                WordTile wordTile = WordTile.CreateWordTile(wordVector.WordHintPair);
                wordTiles.Add(wordTile);
                wordTile.SetupTile(this);
                if (Application.isPlaying) _wordConnectManager.StateUpdated += wordTile.GameStateUpdated;

                // Iterate over each occupied position and apply the letter.
                for (int i = 0; i < wordVector.OccupiedPositions.Count; i++)
                {
                    // Calculate the grid positions.
                    Vector2Int position = wordVector.OccupiedPositions[i];
                    Vector2Int newPosition = new Vector2Int(position.x + vectorOffset.x, position.y + vectorOffset.y);

                    // If already existing, link the existing tile to the word tile.
                    if (temporaryGrid[newPosition.x, newPosition.y] != null)
                    {
                        // Temporary declaration for the overlapping tile and word.
                        LetterTile overlappingTile = temporaryGrid[newPosition.x, newPosition.y].GetComponent<LetterTile>();
                        WordTile overlappingWordTile = wordTiles.Find(x => x.ContainsLetterTile(overlappingTile));

                        // Save the existing letter.
                        wordTile.AddLetter(overlappingTile);

                        // Link the overlapping elements to eachother.
                        wordTile.ConnectOverlappingWordTile(overlappingTile, overlappingWordTile);
                        overlappingWordTile.ConnectOverlappingWordTile(overlappingTile, wordTile);
                        continue;
                    }

                    // Create the letter tile object.
                    GameObject tile = Instantiate(_letterTile, parent.transform);
                    tile.name = $"Letter [{newPosition.x}, {newPosition.y}]";

                    // Update the letter tile.
                    LetterTile letterTile = tile.GetComponent<LetterTile>();
                    letterTile.UpdateColors();
                    letterTile.CompletionAnimationScale = AnimationTileScale;
                    letterTile.LetterDisplay.text = wordVector.WordHintPair.Word[i].ToString().ToUpper();

                    // Save the letter tile class for quick usage later on and add to the managing word tile.
                    letterTiles.Add(letterTile);
                    wordTile.AddLetter(letterTile);

                    // Set the grid data.
                    temporaryGrid[newPosition.x, newPosition.y] = tile;
                    temporaryPreviewGrid[newPosition.x, newPosition.y] = wordVector.WordHintPair.Word[i];
                }

                // Iterate over each influenced position and mark it with an $ (if it fits).
                for (int i = 0; i < wordVector.InfluencedPositions.Count; i++)
                {
                    Vector2Int position = wordVector.InfluencedPositions[i];
                    int xPos = position.x + vectorOffset.x;
                    int yPos = position.y + vectorOffset.y;

                    if (dimensions.x > xPos && xPos >= 0 && dimensions.y > yPos && yPos >= 0)
                        if (temporaryPreviewGrid[xPos, yPos] == '\0')
                            temporaryPreviewGrid[xPos, yPos] = '$';
                }
            }

            // Fill in the empty space of the grid and order the tiles.
            for (int x = 0; x < dimensions.x; x++)
            {
                //Iterate backwards through the y dimension, as we want to build the hierachy from top downwards to get the correct sorting order.
                for (int y = dimensions.y - 1; y >= 0; y--)
                {
                    // If empty, create a blocked tile object, otherwise create the filled letter.
                    if (temporaryGrid[x, y] == null)
                    {
                        // Create the blocked tile object.
                        GameObject tile = Instantiate(_blockedTile, parent.transform);
                        tile.GetComponent<Image>().color = new Color();
                        tile.name = $"Block [{x}, {y}]";
                        temporaryGrid[x, y] = tile;
                    }
                    else
                    {
                        // Reorder the existing tile to the correct index.
                        temporaryGrid[x, y].transform.SetAsLastSibling();
                    }
                }
            }

            _gameLayout.SetGridTiles(temporaryGrid);
            _gameLayout.SetGridLetters(temporaryPreviewGrid);
        }

        /// <summary>
        /// Builds the active layout preview of the crossword, requires the active layout to have been built first.
        /// </summary>
        private void BuildLayoutPreview()
        {
            // Remove old child/grid objects.
            for (int i = _previewGridLayout.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(_previewGridLayout.transform.GetChild(i).gameObject);

            // Set the dimensions of the layout and crossword.
            int width = _gameLayout.GridLetters.GetLength(0);
            int height = _gameLayout.GridLetters.GetLength(1);

            // Set the grid contstraints to be correct.
            _previewGridLayout.constraintCount = width;

            // Create new layout objects.
            for (int x = 0; x < width; x++)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    // If empty, create a blocked tile object, otherwise create the filled letter.
                    if (_gameLayout.GridLetters[x, y] == '\0')
                    {
                        // Create the blocked tile object.
                        GameObject tile = Instantiate(_blockedTile, _previewGridLayout.transform);
                        tile.name = $"Preview Block [{x}, {y}]";
                    }
                    else if (_gameLayout.GridLetters[x, y] == '$')
                    {
                        // Create the influence block tile object.
                        GameObject tile = Instantiate(_blockedTile, _previewGridLayout.transform);
                        tile.name = $"Preview Influence [{x}, {y}]";

                        // Color it for preview.
                        Image image = tile.GetComponent<Image>();
                        image.color = new Color(0.85f,0.25f,0.25f);
                    }
                    else
                    {
                        // Create the letter tile object.
                        GameObject tile = Instantiate(_letterTile, _previewGridLayout.transform);
                        tile.name = $"Preview Letter [{x}, {y}]";

                        // Update the letter tile letter for previewing purposes.
                        LetterTile letterTile = tile.GetComponent<LetterTile>();

                        // Set the colors of the tile for previewing purposes.
                        letterTile.LetterDisplay.color = Color.black;
                        letterTile.BackgroundImage.color = Color.white;

                        // Set letter value of the tile.
                        letterTile.LetterDisplay.text = _gameLayout.GridLetters[x, y].ToString().ToUpper();
                    }
                }
            }
        }
    }
}

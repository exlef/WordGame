using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DTT.Tweening;
using System;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// This class will display the description of a word when needed.
    /// </summary>
    public class HintDescriptionUI : StateUpdateUI
    {
        /// <summary>
        /// Reference to the grid manager script to find the position of the hinted word.
        /// </summary>
        [SerializeField]
        private WordConnectGridManager _gridManager;

        /// <summary>
        /// Reference to the scroll controller to scroll to certain tiles in the viewport.
        /// </summary>
        [SerializeField]
        private ScrollContentController _scrollController;

        /// <summary>
        /// The gameobject containing all UI elements.
        /// </summary>
        [SerializeField]
        private GameObject _descriptionFullUI;

        /// <summary>
        /// The gameobject containing the UI popup.
        /// </summary>
        [SerializeField]
        private RectTransform _descriptionPopup;

        /// <summary>
        /// The gameobject which points to the hinted word.
        /// </summary>
        [SerializeField]
        private GameObject _popupArrow;

        /// <summary>
        /// The hint indicator on the popup window.
        /// </summary>
        [SerializeField]
        private GameObject _hintIndicator;

        /// <summary>
        /// The hint icon on the popup window.
        /// </summary>
        [SerializeField]
        private GameObject _hintIcon;

        /// <summary>
        /// The canvas group attached to the UI to control the alpha.
        /// </summary>
        [SerializeField]
        private CanvasGroup _canvasGroup;

        /// <summary>
        /// The textfield which will be updated with the description of a word.
        /// </summary>
        [SerializeField]
        private Text _descriptionTextField;

        /// <summary>
        /// Container for the letter tile objects.
        /// </summary>
        [SerializeField]
        private GridLayoutGroup _highlightGridLayout;

        /// <summary>
        /// List of word tiles in the highlight grid.
        /// </summary>
        private List<WordTile> _highlightWordTiles;

        /// <summary>
        /// List of canvas groups attached to the letter tiles to control which tiles to show or hide.
        /// </summary>
        private Dictionary<LetterTile, CanvasGroup> _highlightTileCanvasGroups;

        /// <summary>
        /// The Button that reveals a descriptive hint.
        /// </summary>
        [SerializeField]
        private Button _revealDescriptiveHintButton;

        /// <summary>
        /// The Button that disables the descriptive hint UI.
        /// </summary>
        [SerializeField]
        private Button _disableUIButton;

        /// <summary>
        /// The descriptive hint currently being displayed by the UI.
        /// </summary>
        private string _currentWordHinted;

        /// <summary>
        /// The textfield which will be updated with the description of a word.
        /// </summary>
        [SerializeField]
        private bool _horizontalHintPopup = false;

        /// <summary>
        /// The current active UI fading animation.
        /// </summary>
        private Coroutine currentAnimation;

        /// <summary>
        /// On enable start listening to events from the game manager and initialize variables.
        /// </summary>
        new private void OnEnable()
        {
            base.OnEnable();
            WordConnectManager.Instance.Cleanup += CleanupUI;
            WordConnectManager.Instance.Started += GenerateHighlightGrid;

            _currentWordHinted = string.Empty;
            _highlightTileCanvasGroups = new Dictionary<LetterTile, CanvasGroup>();
        }

        /// <summary>
        /// On disable stop listening to the cleanup event.
        /// </summary>
        new private void OnDisable()
        {
            base.OnDisable();
            WordConnectManager.Instance.Cleanup -= CleanupUI;
            WordConnectManager.Instance.Started -= GenerateHighlightGrid;
        }

        /// <summary>
        /// Once a game state occurs, display the chosen word's description.
        /// </summary>
        /// <param name="state">The updated game state data.</param>
        public override void UpdateUI(WordConnectState state)
        {
            // If no descriptive hint has been assigned.
            if (state.CurrentRevealedDescriptiveHint == null)
            {
                _hintIndicator.SetActive(false);
                if(!string.IsNullOrWhiteSpace(_currentWordHinted))
                {
                    // The currently shown descriptive hint has been assigned, but it has been found
                    // The hint button will now generate a new hint on click.
                    _revealDescriptiveHintButton.onClick.RemoveListener(EnableUI);
                }
                return;
            }

            // If the current hinted word does not match the one being displayed, rebuild the UI and enable it. 
            if (!KurmanjiTextUtility.WordsEqual(state.CurrentRevealedDescriptiveHint.Word, _currentWordHinted))
            {
                // Set the text field to the revealed hint description.
                _descriptionTextField.text = state.CurrentRevealedDescriptiveHint.WordDescription;
                _currentWordHinted = state.CurrentRevealedDescriptiveHint.Word;

                // Change the onclick behaviour to revealing the currently active hint, instead of generating a new hint.
                _revealDescriptiveHintButton.onClick.AddListener(EnableUI);
                EnableUI();
            }
        }

        /// <summary>
        /// Builds the grid which displays over the dimmed overlay.
        /// </summary>
        private void GenerateHighlightGrid()
        {
            // Let the grid manager build another copy of the game grid which will be used to highlight.
            _gridManager.BuildLayout(_highlightGridLayout, out _highlightWordTiles, out _);

            // Loop over all the word tiles and lettertiles of the newly generated grid.
            foreach(WordTile wordTile in _highlightWordTiles)
            {
                foreach (LetterTile letterTile in wordTile.LetterTiles)
                {
                    CanvasGroup canvasGroup = letterTile.GetComponent<CanvasGroup>();
                    canvasGroup.alpha = 0;
                    // Save the canvas group to a dictionary together with the letter tile if it hasn't been added yet.
                    if(!_highlightTileCanvasGroups.ContainsKey(letterTile))
                        _highlightTileCanvasGroups.Add(letterTile, canvasGroup);
                }
            }
        }

        /// <summary>
        /// Highlights the currently hinted at letter tiles.
        /// </summary>
        private void HighlightTiles()
        {
            string currentHintWord = WordConnectManager.Instance.WordConnectState.CurrentRevealedDescriptiveHint.Word;

            // Get the currently hinted at WordTile object.
            WordTile wordTile = GetHintedWordTile(_highlightWordTiles);
            if(wordTile == null)
            {
                Debug.LogErrorFormat("ERROR: Tried retrieving a WordTile object where Word == {0}, but it does not exist.", currentHintWord);
                return;
            }

            // Loop over all the letter tiles in the currently hinted word.
            foreach (LetterTile letterTile in wordTile.LetterTiles)
            {
                // Display the letter tile.
                _highlightTileCanvasGroups[letterTile].alpha = 1;
            }
        }

        /// <summary>
        /// Discards level specific generated UI and clears variables for use in the next level.
        /// </summary>
        public void CleanupUI()
        {
            // Disable the hint indicator object and reset its parent as it might persist through levels.
            _hintIndicator.transform.SetParent(_descriptionFullUI.transform);
            _hintIndicator.SetActive(false);

            _gridManager.DisposeWordTiles(_highlightWordTiles, true);

            // Clear the grid dictionary.
            _highlightTileCanvasGroups.Clear();
        }

        /// <summary>
        /// Sets all the layout options to position the popup and point the arrow to the correct tile.
        /// </summary>
        private void PointUIToHintedWord(WordTile wordTile)
        {
            // Get the last letter tile in the found word tile. The last letter is always the furthest right / bottom.
            // We use the last letter in a word so the popup won't overlay on top of tiles which require highlighting
            LetterTile letterTileToPointTo = wordTile.LetterTiles[wordTile.LetterTiles.Count - 1];

            // Update the position and scale of the highlight grid to match the game grid.
            _highlightGridLayout.transform.position = _gridManager.GridLayout.transform.position;
            _highlightGridLayout.transform.localScale = _gridManager.GridLayout.transform.localScale;

            // Stop the scroll rect from scrolling as this might change while overlaying the highlight grid.
            _scrollController.ScrollRect.StopMovement();

            // Set the hint indicator button position to the top left corner position of the first letter tile.
            _hintIndicator.transform.SetParent(wordTile.LetterTiles[0].transform);
            _hintIndicator.transform.localPosition = new Vector3(-_gridManager.GridLayout.cellSize.x / 2, _gridManager.GridLayout.cellSize.y / 2, 0f);

            Vector3 hintUIOffset = _horizontalHintPopup ? new Vector3(65f, 0f, 0) : new Vector3(0, -65f, 0);
            // Set the hint popup and the arrow position to the letter tile position with an offset for spacing.
            Vector3 popupPosition = letterTileToPointTo.transform.TransformPoint(hintUIOffset);

            Vector3 pivotUI = _horizontalHintPopup ? new Vector2(0, 0.5f) : new Vector2(0.5f, 1);
            // Set the popup pivot to match the desired axis of movement.
            _descriptionPopup.pivot = pivotUI;

            // If the popup should move on the horizontal axis. (landscape)
            if (_horizontalHintPopup)
            {
                // Rotate the arrow as the pointing is done horizontally.
                _popupArrow.transform.rotation = Quaternion.Euler(0, 0, 90);

                // Apply calculated positions
                _descriptionPopup.position = popupPosition;
                _popupArrow.transform.position = popupPosition;
            }
            // If the popup should move on the vertical axis. (portrait)
            else
            {
                // Set the y position of the hint popup to the calculated position.
                _descriptionPopup.position = new Vector3(_descriptionPopup.position.x, popupPosition.y, 0);
                _popupArrow.transform.position = popupPosition;

                // If the arrow is on the right side of the hint popup, put the hint icon on the left side, vice versa. This way they dont overlap.
                if (_popupArrow.transform.localPosition.x > 0)
                    _hintIcon.transform.localPosition = new Vector3(-250f, -10, 0f);
                else
                    _hintIcon.transform.localPosition = new Vector3(250f, -10, 0f);
            }
        }

        /// <summary>
        /// Gets the wordtile which represents the hinted word in the given list of word tiles.
        /// </summary>
        /// 
        /// ="wordTiles">The list of word tiles to search for the hinted word.</param>
        /// <returns>The WordTile which represents the hinted word.</returns>
        private WordTile GetHintedWordTile(List<WordTile> wordTiles)
        {
            string hintedWord = WordConnectManager.Instance.WordConnectState.CurrentRevealedDescriptiveHint.Word;
            return wordTiles.Find(tile => KurmanjiTextUtility.WordsEqual(tile.WordHint.Word, hintedWord));
        }

        /// <summary>
        /// Enables the hint description UI.
        /// </summary>
        public void EnableUI()
        {
            // Get the WordTile object which represents the hint being displayed.
            WordTile wordTile = GetHintedWordTile(_gridManager.WordTiles);

            // Get the last letter tile in the found word tile. The last letter is always the furthest right / bottom.
            // We use the last letter in a word so the popup won't overlay on top of tiles which require highlighting
            LetterTile letterTileToPointTo = wordTile.LetterTiles[wordTile.LetterTiles.Count - 1];

            // If an animation is already playing, stop it.
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);
            // If a scroll system is being used, scroll to the highlighted tiles to ensure they are visible.
            if (_scrollController.ScaleGridLayoutToFit)
            {
                // Setup the UI settings
                PointUIToHintedWord(wordTile);
                _descriptionFullUI.SetActive(true);
                //Start fading in the UI without delay.
                currentAnimation = StartCoroutine(DTTween.ValueC(0f, 1f, 0.5f, 0f, Easing.EASE_OUT_EXPO, (value) => _canvasGroup.alpha = value, () => _canvasGroup.alpha = 1f));
            }
            else
            {
                // Disable the hint button which calles this this function from being clicked again.
                _revealDescriptiveHintButton.interactable = false;

                // Delegate which is called on completion of the scroll animation.
                Action onCompleteScrolling = () =>
                {
                    _descriptionFullUI.SetActive(true);
                    _revealDescriptiveHintButton.interactable = true;
                    // Setup the layout settings. 
                    PointUIToHintedWord(wordTile);
                    // Start fading in the UI.
                    currentAnimation = StartCoroutine(DTTween.ValueC(0f, 1f, 0.5f, 0, Easing.EASE_OUT_EXPO, (value) => _canvasGroup.alpha = value, () => _canvasGroup.alpha = 1f));
                };

                // Animate the scroll rect to show the hinted tiles.
                // After completion setup and start fading in the UI. 
                _scrollController.AnimateScrollRectToPoint(0.25f, letterTileToPointTo.transform.position, onCompleteScrolling);
                
            }

            // Highlight the hinted tiles.
            HighlightTiles();

            // Disable the hint indicator button on the grid.
            _hintIndicator.SetActive(false);
        }

        /// <summary>
        /// Disables the UI Hint description UI.
        /// </summary>
        public void DisableUI()
        {
            // Remove all highlighting of the tiles.
            foreach (KeyValuePair<LetterTile, CanvasGroup> tile in _highlightTileCanvasGroups)
                if(tile.Value != null)
                    tile.Value.alpha = 0;

            // Enable the hint indicator on the grid.
            _hintIndicator.SetActive(true);
            _disableUIButton.interactable = false;

            // The delegate called once te UI fading out has completed.
            Action onComplete = () =>
            {
                _descriptionFullUI.SetActive(false);
                _disableUIButton.interactable = true;
            };

            // Start fading out the UI.
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(DTTween.ValueC(1f, 0f, 0.5f, 0f, Easing.EASE_OUT_EXPO, (value) => _canvasGroup.alpha = value, onComplete));

            // If a scroll rect is being used, move it back to its original position.
            if(!_scrollController.ScaleGridLayoutToFit)
                _scrollController.AnimateScrollRectToPoint(0.25f, _scrollController.ScrollRect.viewport.transform.position, null);
        }
    }
}

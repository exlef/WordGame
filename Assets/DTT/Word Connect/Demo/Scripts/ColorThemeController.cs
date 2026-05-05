using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WordGame;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Controls the colors of the game using the assigned game configuration.
    /// </summary>
    public class ColorThemeController : MonoBehaviour
    {
        [Header("Standard elements")]
        /// <summary>
        /// List of image components to be colored with the background color.
        /// </summary>
        [SerializeField]
        private Image _background;

        /// <summary>
        /// The background gradient image component.
        /// </summary>
        [SerializeField]
        private Image _backgroundGradient;

        /// <summary>
        /// The background details image component.
        /// </summary>
        [SerializeField]
        private RawImage _backgroundElements;

        /// <summary>
        /// List of hint button background components.
        /// </summary>
        [SerializeField]
        private List<Image> _hintButtons;

        /// <summary>
        /// List of hint button highlight background components.
        /// </summary>
        [SerializeField]
        private List<Image> _hintHighlights;

        /// <summary>
        /// Reference to the letter input button to change the letter input button colors.
        /// </summary>
        [SerializeField]
        private LetterInputButton _letterInputPrefab;

        /// <summary>
        /// Reference to the letter input button to change the letter input button colors.
        /// </summary>
        [SerializeField]
        private InputLine _inputLinePrefab;

        /// <summary>
        /// Reference to the letter tile prefab to change the state colors.
        /// </summary>
        [SerializeField]
        private LetterTile _letterTilePrefab;

        [Header("Custom elements")]
        /// <summary>
        /// List of all UI elements which should be colored with the hightlight color.
        /// </summary>
        [SerializeField]
        private List<Image> _highlightElements;

        /// <summary>
        /// List of all UI elements which should be colored with the light hightlight color.
        /// </summary>
        [SerializeField]
        private List<Image> _lightHighlightElements;

        /// <summary>
        /// List of all UI elements which should be colored with the global background color.
        /// </summary>
        [SerializeField]
        private List<Image> _backgroundColoredElements;

        /// <summary>
        /// On start listen to the initialize event to update the game colors.
        /// </summary>
        private void OnEnable()
        {
            if (WordConnectManager.Instance != null)
                WordConnectManager.Instance.Initialized += UpdateColors;
        }

        /// <summary>
        /// On start stop listening to event.
        /// </summary>
        private void OnDisable()
        {
            if (WordConnectManager.Instance != null)
                WordConnectManager.Instance.Initialized -= UpdateColors;
        }

        /// <summary>
        /// Update the UI to match the global config colors and current level background.
        /// </summary>
        public void UpdateColors()
        {
            WordConnectManager manager = WordConnectManager.Instance;
            GameConfig gameConfig = manager != null ? manager.GameConfig : null;

            if (gameConfig == null)
                return;

            GameConfig.LevelBackgroundColors backgroundColors = gameConfig.GetBackgroundColorsForLevel(manager.CurrentLevelNumber);

            // Set the background colors.
            _background.color = backgroundColors.BackgroundColor;
            _backgroundGradient.color = backgroundColors.BackgroundGradientColor;
            _backgroundElements.color = backgroundColors.BackgroundElementsColor;

            // Set the hint button colors.
            _hintButtons.ForEach((button) => button.color = gameConfig.HintButtonColor);
            _hintHighlights.ForEach((button) => button.color = gameConfig.LightHintButtonColor);

            // Set the highlight UI elements colors.
            _highlightElements.ForEach((element) => element.color = gameConfig.HighlightColor);
            _lightHighlightElements.ForEach((element) => element.color = gameConfig.LightHighlightColor);
            _backgroundColoredElements.ForEach((element) => element.color = gameConfig.GlobalBackgroundColor);

            // Set letter input colors.
            _letterInputPrefab.SetColors(gameConfig.HighlightColor, gameConfig.LetterInputLetterColor);
            _inputLinePrefab.SetColor(gameConfig.HighlightColor);

            // The regular text color should be equal to the found text color with the alpha set to zero.
            // This way when animating to the found state only the alpha animates.
            Color regularTextColor = gameConfig.FoundTileLetterColor;
            regularTextColor.a = 0;

            // Set the letter tile state colors.
            _letterTilePrefab.SetColors(regularTextColor, gameConfig.RegularTileColor, gameConfig.HintedTileLetterColor, gameConfig.HintedTileColor, gameConfig.FoundTileLetterColor, gameConfig.FoundTileColor);
        }
    }
}

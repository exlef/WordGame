using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Controls the colors of the game using the color theme in the given game configuration.
    /// </summary>
    public class ColorThemeController : MonoBehaviour
    {
        /// <summary>
        /// The background image component.
        /// </summary>
        [SerializeField]
        private ColorTheme _standardColorTheme;

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
        /// List of all UI elements which should be colored with the background color.
        [SerializeField]
        private List<Image> _backgroundColoredElements;

        /// <summary>
        /// On start listen to the initialize event to update the game colors.
        /// </summary>
        private void OnEnable() => WordConnectManager.Instance.Initialized += () => UpdateColors(WordConnectManager.Instance.Configuration.ColorTheme);

        /// <summary>
        /// On start stop listening to event.
        /// </summary>
        private void OnDisable() => WordConnectManager.Instance.Initialized -= () => UpdateColors(WordConnectManager.Instance.Configuration.ColorTheme);

        /// <summary>
        /// Update the entire UI to match the given color theme.
        /// </summary>
        /// <param name="colorTheme">The color theme which contains the colors which the UI will change to.</param>
        public void UpdateColors(ColorTheme colorTheme)
        {
            // If there is no color theme, return.
            if (colorTheme == null)
                colorTheme = _standardColorTheme;

            // Set the background colors
            _background.color = colorTheme.BackgroundColor;
            _backgroundGradient.color = colorTheme.BackgroundGradientColor;
            _backgroundElements.color = colorTheme.BackgroundElementsColor;

            // Set the hint button colors.
            _hintButtons.ForEach((button) => button.color = colorTheme.HintButtonColor);
            _hintHighlights.ForEach((button) => button.color = colorTheme.LightHintButtonColor);

            // Set the highlight UI elements colors.
            _highlightElements.ForEach((element) => element.color = colorTheme.HighlightColor);
            _lightHighlightElements.ForEach((element) => element.color = colorTheme.LightHighlightColor);
            _backgroundColoredElements.ForEach((element) => element.color = colorTheme.BackgroundColor);

            // Set letter input colors.
            _letterInputPrefab.SetColors(colorTheme.HighlightColor, colorTheme.LetterInputLetterColor); 
            _inputLinePrefab.SetColor(colorTheme.HighlightColor);

            // The regular text color should be equal to the found text color with the alpha set to zero.
            // This way when animating to the found state only the alpha animates.
            Color regularTextColor = colorTheme.FoundTileLetterColor;
            regularTextColor.a = 0;

            // Set the letter tile state colors.
            _letterTilePrefab.SetColors(regularTextColor, colorTheme.RegularTileColor, colorTheme.HintedTileLetterColor, colorTheme.HintedTileColor, colorTheme.FoundTileLetterColor, colorTheme.FoundTileColor);
        }
    }
}

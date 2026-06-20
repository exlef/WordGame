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
            SetImageColor(_background, backgroundColors.BackgroundColor);
            SetImageColor(_backgroundGradient, backgroundColors.BackgroundGradientColor);
            SetRawImageColor(_backgroundElements, backgroundColors.BackgroundElementsColor);

            // Set the hint button colors.
            SetImageColors(_hintButtons, gameConfig.HintButtonColor);
            SetImageColors(_hintHighlights, gameConfig.LightHintButtonColor);

            // Set the highlight UI elements colors.
            SetImageColors(_highlightElements, gameConfig.HighlightColor);
            SetImageColors(_lightHighlightElements, gameConfig.LightHighlightColor);
            SetImageColors(_backgroundColoredElements, gameConfig.GlobalBackgroundColor);

            // Set letter input colors.
            if (_letterInputPrefab != null)
                _letterInputPrefab.SetColors(gameConfig.HighlightColor, gameConfig.LetterInputLetterColor);

            if (_inputLinePrefab != null)
                _inputLinePrefab.SetColor(gameConfig.HighlightColor);

            // The regular text color should be equal to the found text color with the alpha set to zero.
            // This way when animating to the found state only the alpha animates.
            Color regularTextColor = gameConfig.FoundTileLetterColor;
            regularTextColor.a = 0;

            // Set the letter tile state colors.
            if (_letterTilePrefab != null)
                _letterTilePrefab.SetColors(regularTextColor, gameConfig.RegularTileColor, gameConfig.HintedTileLetterColor, gameConfig.HintedTileColor, gameConfig.FoundTileLetterColor, gameConfig.FoundTileColor);
        }

        /// <summary>
        /// Sets the color of an image if it still exists in the scene.
        /// </summary>
        /// <param name="image">Image to color.</param>
        /// <param name="color">Color to apply.</param>
        private static void SetImageColor(Image image, Color color)
        {
            if (image != null)
                image.color = color;
        }

        /// <summary>
        /// Sets the color of a raw image if it still exists in the scene.
        /// </summary>
        /// <param name="image">Raw image to color.</param>
        /// <param name="color">Color to apply.</param>
        private static void SetRawImageColor(RawImage image, Color color)
        {
            if (image != null)
                image.color = color;
        }

        /// <summary>
        /// Sets the color of images that still exist in the scene.
        /// </summary>
        /// <param name="images">Images to color.</param>
        /// <param name="color">Color to apply.</param>
        private static void SetImageColors(List<Image> images, Color color)
        {
            if (images == null)
                return;

            for (int i = 0; i < images.Count; i++)
                SetImageColor(images[i], color);
        }
    }
}

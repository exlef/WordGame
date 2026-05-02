using DTT.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Class that represents and manages the data of a single letter in a Word Connect game.
    /// </summary>
    public class LetterTile : MonoBehaviour
    {
        /// <summary>
        /// The Text field this letter tile has attached.
        /// </summary>
        [field: SerializeField]
        public Text LetterDisplay { get; private set; }

        /// <summary>
        /// The Text field this letter tile has attached.
        /// </summary>
        [field: SerializeField]
        public Image BackgroundImage { get; private set; }

        /// <summary>
        /// The canvas group attached to the tile to control the visibility of the tile.
        /// </summary>
        [field: SerializeField]
        public CanvasGroup CanvasGroup { get; private set; }

        /// <summary>
        /// Keep track whether this letter has been hinted at.
        /// </summary>
        public bool IsHinted { get; set; } = false;

        /// <summary>
        /// Keep track whether this letter has been found.
        /// </summary>
        public bool IsFound { get; set; } = false;

        /// <summary>
        /// Color of the tiles text in the normal state.
        /// Make sure this color has an alpha value of zero, or else the answer will be visible.
        /// </summary>
        [SerializeField]
        private Color _textColor;

        /// <summary>
        /// Color of the tile in the normal state.
        /// </summary>
        [SerializeField]
        private Color _tileColor;

        /// <summary>
        /// Color of the tiles text in the hinted state.
        /// </summary>
        [SerializeField]
        private Color _hintedTextColor;

        /// <summary>
        /// Color of the tile in the hinted state.
        /// </summary>
        [SerializeField]
        private Color _hintedTileColor;

        /// <summary>
        /// Color of the tiles text in the found state.
        /// </summary>
        [SerializeField]
        private Color _foundTextColor;

        /// <summary>
        /// Color of the tile in the found state.
        /// </summary>
        [SerializeField]
        private Color _foundTileColor;

        /// <summary>
        /// The scale this tile will animate to when the word has been found.
        /// </summary>
        public float CompletionAnimationScale { set; private get; } = 1.2f;

        /// <summary>
        /// Sets the colors for this tile should be during its different states.
        /// </summary>
        /// <param name="textColor">Color of the tiles text in the regular state. Should have a alpha value of zero</param>
        /// <param name="tileColor">Color of the tile in the regular state.</param>
        /// <param name="hintTextColor">Color of the tiles text in the hinted state.</param>
        /// <param name="hintTileColor">Color of the tile in the hinted state.</param>
        /// <param name="foundTextColor">Color of the tiles text in the found state.</param>
        /// <param name="foundTileColor">Color of the tile in the found state.</param>
        public void SetColors(Color textColor, Color tileColor, Color hintTextColor, Color hintTileColor, Color foundTextColor, Color foundTileColor)
        {
            _textColor = textColor;
            _tileColor = tileColor;
            _hintedTextColor = hintTextColor;
            _hintedTileColor = hintTileColor;
            _foundTextColor = foundTextColor;
            _foundTileColor = foundTileColor;
        }

        /// <summary>
        /// Updates the tiles colors to the current state colors.
        /// </summary>
        public void UpdateColors()
        {
            // Get the colors of the current state.
            (Color, Color) stateColors = GetStateColors();

            LetterDisplay.color = stateColors.Item1;
            BackgroundImage.color = stateColors.Item2;
        }

        /// <summary>
        /// Gets the colors for the the current state of the tile.
        /// </summary>
        /// <returns>Tuple where item 1 is the text color and item 2 is the tile color.</returns>
        private (Color,Color) GetStateColors()
        {
            // Get the colors of this tile for its current state. Found state has the highest priority.
            if (IsFound)
                return (_foundTextColor, _foundTileColor);

            if (IsHinted)
                return (_hintedTextColor, _hintedTileColor);

            return (_textColor, _tileColor);
        }

        /// <summary>
        /// Animates the colors of the tile background and tile text over a period of time.
        /// </summary>
        /// <param name="animationDuration">The duration of the animation is seconds.</param>
        public void AnimateColors(float animationDuration) 
        {
            Color startTextColor = LetterDisplay.color;
            Color startTileColor = BackgroundImage.color;

            // Get the colors of the tiles current state.
            (Color, Color) stateColors = GetStateColors();
            Color endTextColor = stateColors.Item1;
            Color endTileColor = stateColors.Item2;

            // The delegate to run each update of the animation.
            Action<float> updateColors = (value) =>
            {
                LetterDisplay.color = Color.Lerp(startTextColor, endTextColor, value);
                BackgroundImage.color = Color.Lerp(startTileColor, endTileColor, value);
            };

            // Run and save the new animation.
            DTTween.Value(0f, 1f, animationDuration, 0f, Easing.EASE_OUT_EXPO, updateColors, () => updateColors(1f));
        }
        
        /// <summary>
        /// Animates the completion animation of this letter over the given duration.
        /// </summary>
        /// <param name="animationDuration">The duration of the animation in seconds.</param>
        /// <param name="animationDelay">The delay before the animation starts in seconds.</param>
        public void AnimateLetterCompletion(float animationDuration, float animationDelay)
        {
            AnimateScale(animationDuration / 2f, animationDelay, CompletionAnimationScale, () => AnimateScale(animationDuration / 2f, 0f, 1f));
        }

        /// <summary>
        /// Animates the scale of this tile over the given duration.
        /// </summary>
        /// <param name="animationDuration">Duration of the animation in seconds.</param>
        /// <param name="animationDelay">Delay in seconds before the animation starts.</param>
        /// <param name="letterScale">The scale the letter tile will animate to.</param>
        /// <param name="callback">Optional delegate which is called after completion of the animation.</param>
        public void AnimateScale(float animationDuration, float animationDelay, float letterScale, Action callback = null)
        {
            Vector3 startScale = gameObject.transform.localScale;
            Vector3 endScale = new Vector3(letterScale, letterScale, 1f);
            // The delegate to run each update of the animation.
            Action<float> updateScale = (value) => transform.localScale = Vector3.Lerp(startScale, endScale, value);
            // The delegate which will be called once the animation completes.
            Action onComplete = () =>
            {
                updateScale(1f);
                callback?.Invoke();
            };

            // Run the animation.
            DTTween.Value(0f, 1f, animationDuration, animationDelay, Easing.EASE_OUT_CUBIC, updateScale, onComplete);
        }
    }
}
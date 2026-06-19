using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DTT.Tweening;
using System;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Controls and stores the Unity components of a letter input GameObject.
    /// </summary>
    public class LetterInputButton : MonoBehaviour
    {
        /// <summary>
        /// The rect transform to change the buttons size and position.
        /// </summary>
        [SerializeField]
        private RectTransform _rectTransform;

        /// <summary>
        /// The background image component used to change color.
        /// </summary>
        [SerializeField]
        private Image _background;

        /// <summary>
        /// The text field which shows which letter this input represents.
        /// </summary>
        [SerializeField]
        private Text _text;

        /// <summary>
        /// EventTrigger which handles touch and mouse interaction.
        /// </summary>
        [SerializeField]
        private EventTrigger _eventTrigger;

        /// <summary>
        /// Bool value to keep track of highlighted state to prevent animations triggering multiple times.
        /// </summary>
        public bool IsHighlighted { get; set; }

        /// <summary>
        /// Bool value to keep track of shuffle state to prevent animations triggering multiple times.
        /// </summary>
        public bool IsShuffling { get; set; }

        /// <summary>
        /// Returns the EventTrigger component on this GameObject.
        /// </summary>
        public EventTrigger Trigger => _eventTrigger;

        /// <summary>
        /// Returns the RectTransform component on this GameObject.
        /// </summary>
        public RectTransform Rect => _rectTransform;

        /// <summary>
        /// The current scaling animation running on this letter input. 
        /// </summary>
        private Coroutine activeScaleAnimation;

        /// <summary>
        /// The current text color animation running on this letter input. 
        /// </summary>
        private Coroutine activeColorAnimation;

        /// <summary>
        /// Duration of the scale animation in seconds.
        /// </summary>
        [SerializeField]
        private float _scaleAnimationDuration = 0.20f;

        /// <summary>
        /// Duration of the text animation in seconds.
        /// </summary>
        [SerializeField]
        private float _textAnimationDuration = 0.10f;

        /// <summary>
        /// The color of the background.
        /// </summary>
        [SerializeField]
        private Color _backgroundColor;

        /// <summary>
        /// The color of the letter.
        /// </summary>
        [SerializeField]
        private Color _textColor;

        /// <summary>
        /// Initialize variables on awake.
        /// </summary>
        private void Awake()
        {
            IsHighlighted = false;
            IsShuffling = false;
        }

        /// <summary>
        /// Sets the text color and background color variables.
        /// </summary>
        /// <param name="backgroundColor">The color of the background when the input is highlighted.</param>
        /// <param name="textColor">The color of the letter.</param>
        public void SetColors(Color backgroundColor, Color textColor)
        {
            _backgroundColor = backgroundColor;
            _textColor = textColor;
        }

        /// <summary>
        /// Updates the UI to represent the colors.
        /// </summary>
        public void UpdateColors()
        {
            _background.color = _backgroundColor;
            _text.color = _textColor;
        }

        /// <summary>
        /// Sets the text value to a single letter.
        /// </summary>
        /// <param name="letter">The letter to show.</param>
        public void SetInputLetterText(char letter) => _text.text = KurmanjiTextUtility.ToDisplayUpper(letter.ToString());

        /// <summary>
        /// Starts the animation to scale the background image.
        /// </summary>
        public void AnimateScale()
        {
            Vector3 startScale = _background.transform.localScale;
            Vector3 endScale = IsHighlighted ? Vector3.one : Vector3.zero;

            // The delegate to run each update of the animation.
            Action<float> updateScale = (value) => _background.transform.localScale = Vector3.Lerp(startScale, endScale, value);

            // If a different animation is currently playing, stop it first.
            if (activeScaleAnimation != null)
            {
                StopCoroutine(activeScaleAnimation);
                activeScaleAnimation = null;
            }

            // Run and save the new animation.
            activeScaleAnimation = StartCoroutine(DTTween.ValueC(0f, 1f, _scaleAnimationDuration, 0f, Easing.EASE_OUT_EXPO, updateScale, () => updateScale(1f)));
        }
        /// <summary>
        /// Starts the animation to change the text color.
        /// </summary>
        public void AnimateTextColor()
        {
            Color startColor = _text.color;
            Color endColor = IsHighlighted ? Color.white : _textColor;

            // The delegate to run each update of the animation.
            Action<float> updateColor = (value) => _text.color = Color.Lerp(startColor, endColor, value);

            // If a different animation is currently playing, stop it first.
            if (activeColorAnimation != null)
            {
                StopCoroutine(activeColorAnimation);
                activeColorAnimation = null;
            }

            // Run and save the new animation.
            activeColorAnimation = StartCoroutine(DTTween.ValueC(0f, 1f, _textAnimationDuration, 0f, Easing.EASE_OUT_EXPO, updateColor, () => updateColor(1f)));
        }

        /// <summary>
        /// Animates the position of a tile over the given duration.
        /// </summary>
        /// <param name="animationDuration">The duration of the animation in seconds.</param>
        /// <param name="newPosition">The position to animate to.</param>
        public void AnimateShuffle(float animationDuration, Vector2 newPosition)
        {
            Vector2 startPosition = _rectTransform.anchoredPosition;

            // The delegate to run each update of the animation.
            Action<float> updatePosition = (value) => _rectTransform.anchoredPosition = Vector3.Lerp(startPosition, newPosition, value);
            // The delegate which is called on completion of the animation.
            // Update the position to the final position and set shuffle state to false.
            Action onComplete = () => 
            {
                updatePosition(1);
                IsShuffling = false;
            };

            // Set shuffle state to true.
            IsShuffling = true;

            // Run the shuffle animation.
            DTTween.Value(0f, 1f, animationDuration, Easing.EASE_OUT_CUBIC, updatePosition, onComplete);
        }
    }
}

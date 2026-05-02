using DTT.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Controller script for a scroll rect.
    /// Can make content scale to fit inside the viewport or scroll to a point in the scroll rect programmatically.
    /// </summary>
    public class ScrollContentController : MonoBehaviour
    {
        /// <summary>
        /// The scroll area for the content.
        /// </summary>
        [SerializeField]
        private ScrollRect _scrollRect;

        /// <summary>
        /// <inheritdoc cref="_scrollRect"/>
        /// </summary>
        public ScrollRect ScrollRect => _scrollRect;

        /// <summary>
        /// The content in the viewport.
        /// </summary>
        [SerializeField]
        private RectTransform _content;

        /// <summary>
        /// <inheritdoc cref="_content"/>
        /// </summary>
        public RectTransform Content => _content;

        /// <summary>
        /// Whether to scale the grid layout to fit inside the viewport. 
        /// </summary>
        [SerializeField]
        private bool _scaleGridLayoutToFit;

        /// <summary>
        /// <inheritdoc cref="_scaleGridLayoutToFit"/>
        /// </summary>
        public bool ScaleGridLayoutToFit => _scaleGridLayoutToFit;

        /// <summary>
        /// On enable listen to the start event to scale the grid to the viewport if desired.
        /// </summary>
        private void OnEnable()
        {
            if (_scaleGridLayoutToFit)
                WordConnectManager.Instance.Started += ScaleLayout;
        }

        /// <summary>
        /// On disable stop listening to start event.
        /// </summary>
        private void OnDisable()
        {
            if (_scaleGridLayoutToFit)
                WordConnectManager.Instance.Started -= ScaleLayout;
        }

        /// <summary>
        /// Scales the content to fit the viewport. (No scrolling)
        /// </summary>
        private void ScaleLayout()
        {
            // Force the canvas to repaint to update UI object rects.
            Canvas.ForceUpdateCanvases();

            // Reset grid layout scale.
            _content.localScale = Vector3.one;
            float scaleToFit;
            float gridAspectRatio = _content.rect.width / _content.rect.height;
            float viewportAspectRatio = _scrollRect.viewport.rect.width / _scrollRect.viewport.rect.height;

            // Calculate the scale between the largest axis.
            if (gridAspectRatio > viewportAspectRatio)
                scaleToFit = _scrollRect.viewport.rect.width / _content.rect.width;
            else
                scaleToFit = _scrollRect.viewport.rect.height / _content.rect.height;

            // Apply the calculated scale.
            _content.localScale = new Vector3(scaleToFit, scaleToFit, 1f);
        }

        /// <summary>
        /// Snaps the scroll rect to position the given position in the center of the scroll rect.
        /// </summary>
        /// <param name="position">The position to snap to.</param>
        public void SnapScrollRectToPoint(Vector3 position)
        {
            // Force the canvas to repaint to update UI object rects.
            Canvas.ForceUpdateCanvases();

            // Calculate the difference between the center of the scroll rect and the given posiion.
            Vector2 offsetPosition = (Vector2)_scrollRect.transform.InverseTransformPoint(_scrollRect.viewport.position) - (Vector2)_scrollRect.transform.InverseTransformPoint(position);
            _scrollRect.viewport.localPosition = offsetPosition;

        }

        /// <summary>
        /// Animates the scroll rect to position the target position in the center of the scroll rect.
        /// </summary>
        /// <param name="animationDuration">The duration of the animation in seconds.</param>
        /// <param name="position">The position which will be moved to the center of the scroll rect.</param>
        /// <param name="callback">Optional delegate which will be called once the animation completes.</param>
        public void AnimateScrollRectToPoint(float animationDuration, Vector3 position, Action callback = null)
        {
            // Force the canvas to repaint to update UI object rects.
            Canvas.ForceUpdateCanvases();

            Vector2 startPosition = _scrollRect.viewport.localPosition;
            // Calculate the difference between the center of the scroll rect and the given posiion.
            Vector2 offsetPosition = (Vector2)_scrollRect.transform.InverseTransformPoint(_scrollRect.viewport.position) - (Vector2)_scrollRect.transform.InverseTransformPoint(position);

            Action<float> updatePosition = (value) => _scrollRect.viewport.localPosition = Vector2.Lerp(startPosition, offsetPosition, value);
            Action onComplete = () =>
            {
                updatePosition(1f);
                callback?.Invoke();
            };

            //Start the animation.
            DTTween.Value(0f, 1f, animationDuration, Easing.EASE_OUT_CUBIC, updatePosition, onComplete);
        }
    }
}
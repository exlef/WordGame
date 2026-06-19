using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Controls the indicators of the scroll rect to show in which direction scrolling is possible.
    /// </summary>
    public class ScrollArrowIndicators : MonoBehaviour
    {
        /// <summary>
        /// Reference to the scroll controller.
        /// </summary>
        [SerializeField]
        private ScrollContentController _scrollController;

        /// <summary>
        /// The leftward indicator of the scroll view.
        /// </summary>
        [SerializeField]
        private GameObject _leftIndicator;

        /// <summary>
        /// The rightward indicator of the scroll view.
        /// </summary>
        [SerializeField]
        private GameObject _rightIndicator;

        /// <summary>
        /// The upward indicator of the scroll view.
        /// </summary>
        [SerializeField]
        private GameObject _upIndicator;

        /// <summary>
        /// The downward indicator of the scroll view.
        /// </summary>
        [SerializeField]
        private GameObject _downIndicator;

        /// <summary>
        /// The scroll rect being tracked.
        /// </summary>
        [SerializeField]
        private ScrollRect _trackedScrollArea;

        /// <summary>
        /// The ratio sensitivity to the edge of the scroll view for toggles to occur.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        private float _scrollSensitivity = 0.95f;

        /// <summary>
        /// On enable disable the scroll indicators.
        /// </summary>
        private void OnEnable()
        {
            _rightIndicator.SetActive(false);
            _leftIndicator.SetActive(false);
            _downIndicator.SetActive(false);
            _upIndicator.SetActive(false);
        }

        /// <summary>
        /// Toggle scroll indicators based on current scroll data.
        /// </summary>
        private void Update()
        {
            // If the content of the scroll rect is being scaled to fit in the viewport, indicators are unnecessary.
            if (_scrollController.ScaleGridLayoutToFit)
                return;

            // Toggle indicators if the content width is greater than the viewport width.
            if (_trackedScrollArea.content.rect.width > _trackedScrollArea.viewport.rect.width)
            {
                // End of scroll area horizontally.
                _rightIndicator.SetActive(_trackedScrollArea.horizontalNormalizedPosition < _scrollSensitivity);

                // Start of scroll area horizontally.
                _leftIndicator.SetActive(_trackedScrollArea.horizontalNormalizedPosition > 1 - _scrollSensitivity);
            }
            else
            {
                _rightIndicator.SetActive(false);
                _leftIndicator.SetActive(false);
            }

            // Toggle indicators if the content height is greater than the viewport height.
            if (_trackedScrollArea.content.rect.height > _trackedScrollArea.viewport.rect.height)
            {
                // End of scroll area vertically.
                _upIndicator.SetActive(_trackedScrollArea.verticalNormalizedPosition < _scrollSensitivity);

                // Start of scroll area vertically.
                _downIndicator.SetActive(_trackedScrollArea.verticalNormalizedPosition > 1 - _scrollSensitivity);
            }
            else
            {
                _upIndicator.SetActive(false);
                _downIndicator.SetActive(false);
            }
        }
    }
}

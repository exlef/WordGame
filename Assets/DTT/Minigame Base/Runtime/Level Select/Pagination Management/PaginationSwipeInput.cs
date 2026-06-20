using UnityEngine;
using UnityEngine.EventSystems;

namespace DTT.MinigameBase.LevelSelect.PaginationManagement
{
    /// <summary>
    /// Handles horizontal swipe gestures for pagination.
    /// </summary>
    [DisallowMultipleComponent]
    internal class PaginationSwipeInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// The pagination controller that should receive page changes.
        /// </summary>
        [SerializeField]
        [Tooltip("The pagination controller that should receive page changes.")]
        private Pagination _pagination;

        /// <summary>
        /// Minimum horizontal pointer movement required before a drag counts as a swipe.
        /// </summary>
        [SerializeField]
        [Tooltip("Minimum horizontal pointer movement required before a drag counts as a swipe.")]
        private float _minimumSwipeDistance = 80f;

        /// <summary>
        /// Maximum vertical movement relative to horizontal movement before the swipe is ignored.
        /// </summary>
        [SerializeField]
        [Tooltip("Maximum vertical movement relative to horizontal movement before the swipe is ignored.")]
        [Range(0f, 1f)]
        private float _maximumVerticalRatio = 0.75f;

        /// <summary>
        /// Pointer position where the current drag started.
        /// </summary>
        private Vector2 _dragStartPosition;

        /// <summary>
        /// Whether a drag is currently being tracked.
        /// </summary>
        private bool _trackingDrag;

        /// <inheritdoc />
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _trackingDrag = true;
            _dragStartPosition = eventData.position;
        }

        /// <inheritdoc />
        public void OnDrag(PointerEventData eventData)
        {
        }

        /// <inheritdoc />
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_trackingDrag)
                return;

            _trackingDrag = false;

            Vector2 swipe = eventData.position - _dragStartPosition;
            float horizontalDistance = Mathf.Abs(swipe.x);
            if (horizontalDistance < _minimumSwipeDistance)
                return;

            if (Mathf.Abs(swipe.y) > horizontalDistance * _maximumVerticalRatio)
                return;

            if (_pagination == null)
                return;

            if (swipe.x < 0)
                _pagination.NextPage();
            else
                _pagination.PreviousPage();
        }
    }
}

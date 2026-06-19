using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// This class controls the displaying and orienting of a line GameObject used to connect an enabled input letter.
    /// </summary>
    public class InputLine : MonoBehaviour
    {
        /// <summary>
        /// Image component used to control the color of the line's background.
        /// </summary>
        [SerializeField]
        private Image _background;

        /// <summary>
        /// <inheritdoc cref="_background"/>
        /// </summary>
        public Image Background => _background;

        /// <summary>
        /// Rect Transform component to control position, rotation and scale on the canvas.
        /// </summary>
        [SerializeField]
        private RectTransform _rect;

        /// <summary>
        /// <inheritdoc cref="_rect"/>
        /// </summary>
        public RectTransform Rect => _rect;

        /// <summary>
        /// Thickness of the line when rendered.
        /// </summary>
        private const float LINE_THICKNESS = 15f;

        /// <summary>
        /// Disables the line's GameObject to stop it from rendering.
        /// </summary>
        public void Disable() => gameObject.SetActive(false);

        /// <summary>
        /// Disables the line's GameObject to stop it from rendering.
        /// </summary>
        public void Enable() => gameObject.SetActive(true);

        /// <summary>
        /// Color of the line.
        /// </summary>
        [SerializeField]
        private Color _lineColor;

        /// <summary>
        /// Sets the color this line should be.
        /// </summary>
        /// <param name="lineColor">The color this line should be.</param>
        public void SetColor(Color lineColor) => _lineColor = lineColor;

        /// <summary>
        /// Updates the line to reflect the color it should be.
        /// </summary>
        public void UpdateColor() => _background.color = _lineColor;

        /// <summary>
        /// Takes in a start and end position which the line will position itself to.
        /// </summary>
        /// <param name="startPosition">The position the line will start at.</param>
        /// <param name="endPosition">The position the line will end at.</param>
        public void SetLine(Vector2 startPosition, Vector2 endPosition)
        {
            // Calculate the angle and distance between start and endposition.
            float angle = Mathf.Atan2(startPosition.y - endPosition.y, startPosition.x - endPosition.x) * Mathf.Rad2Deg;
            float distance = Vector2.Distance(startPosition, endPosition);

            // Set the position, rotation and scale of the line to orient along the given start and end position.
            Rect.anchoredPosition = startPosition;
            Rect.rotation = Quaternion.Euler(0, 0, angle + 90f);
            Rect.sizeDelta = new Vector2(LINE_THICKNESS, distance);
        }
    }
}
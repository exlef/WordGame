using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Creates and updates rewarded-ad badge icons on hint buttons.
    /// </summary>
    internal static class HintAdBadgeUtility
    {
        /// <summary>
        /// Replaces a numeric hint label with a rewarded-ad icon.
        /// </summary>
        /// <param name="textField">The old numeric label.</param>
        /// <param name="iconSprite">The sprite used for the rewarded-ad icon.</param>
        /// <param name="iconImage">Cached icon image instance.</param>
        public static void ShowAdBadge(Text textField, Sprite iconSprite, ref Image iconImage)
        {
            if (textField == null)
                return;

            textField.text = string.Empty;
            textField.raycastTarget = false;

            if (iconImage == null && iconSprite != null)
                iconImage = CreateIcon(textField, iconSprite);

            if (iconImage == null)
                return;

            iconImage.sprite = iconSprite;
            iconImage.enabled = true;
        }

        /// <summary>
        /// Creates an icon image in the old label's rect.
        /// </summary>
        /// <param name="textField">The old numeric label.</param>
        /// <param name="iconSprite">The sprite used for the rewarded-ad icon.</param>
        /// <returns>The created image.</returns>
        private static Image CreateIcon(Text textField, Sprite iconSprite)
        {
            GameObject iconObject = new GameObject("Rewarded Ad Badge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(textField.transform, false);

            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = iconObject.GetComponent<Image>();
            image.sprite = iconSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }
    }
}

using UnityEngine;
using UnityEngine.Serialization;

namespace WordGame
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "WordGame/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField]
        [FormerlySerializedAs("primaryFont")]
        private Font regularFont;

        [SerializeField]
        private Font boldFont;

        public Font PrimaryFont => regularFont;

        public Font RegularFont => regularFont;

        public Font BoldFont => boldFont;

        public Font ResolveFont(FontStyle fontStyle)
        {
            if (IsBoldStyle(fontStyle))
                return boldFont;

            return regularFont;
        }

        public FontStyle ResolveFontStyle(FontStyle fontStyle)
        {
            if (IsBoldStyle(fontStyle) && boldFont != null)
                return fontStyle == FontStyle.BoldAndItalic ? FontStyle.Italic : FontStyle.Normal;

            return fontStyle;
        }

        private static bool IsBoldStyle(FontStyle fontStyle) =>
            fontStyle == FontStyle.Bold || fontStyle == FontStyle.BoldAndItalic;
    }
}

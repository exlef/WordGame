using UnityEngine;
using UnityEngine.Serialization;

namespace WordGame
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "WordGame/Game Config")]
    public class GameConfig : ScriptableObject
    {
        public const int DefaultDescriptionHintCost = 1;
        public const int DefaultLetterHintCost = 2;
        public const int DefaultMinWordHintCost = 3;
        public const float DefaultThreeStarMaxAssistRatio = 0.15f;
        public const float DefaultTwoStarMaxAssistRatio = 0.35f;
        public static readonly Color DefaultResultEarnedStarColor = new Color32(255, 198, 0, 255);
        public static readonly Color DefaultResultUnearnedStarColor = new Color32(216, 224, 240, 255);

        [SerializeField]
        [FormerlySerializedAs("primaryFont")]
        private Font regularFont;

        [SerializeField]
        private Font boldFont;

        [Header("Scoring")]
        [SerializeField]
        [Min(0)]
        private int descriptionHintCost = DefaultDescriptionHintCost;

        [SerializeField]
        [Min(0)]
        private int letterHintCost = DefaultLetterHintCost;

        [SerializeField]
        [Min(0)]
        private int minWordHintCost = DefaultMinWordHintCost;

        [SerializeField]
        [Range(0f, 1f)]
        private float threeStarMaxAssistRatio = DefaultThreeStarMaxAssistRatio;

        [SerializeField]
        [Range(0f, 1f)]
        private float twoStarMaxAssistRatio = DefaultTwoStarMaxAssistRatio;

        [Header("Results")]
        [SerializeField]
        private Color resultEarnedStarColor = DefaultResultEarnedStarColor;

        [SerializeField]
        private Color resultUnearnedStarColor = DefaultResultUnearnedStarColor;

        public Font PrimaryFont => regularFont;

        public Font RegularFont => regularFont;

        public Font BoldFont => boldFont;

        public int DescriptionHintCost => Mathf.Max(0, descriptionHintCost);

        public int LetterHintCost => Mathf.Max(0, letterHintCost);

        public int MinWordHintCost => Mathf.Max(0, minWordHintCost);

        public float ThreeStarMaxAssistRatio => Mathf.Clamp01(threeStarMaxAssistRatio);

        public float TwoStarMaxAssistRatio => Mathf.Clamp01(Mathf.Max(twoStarMaxAssistRatio, threeStarMaxAssistRatio));

        public Color ResultEarnedStarColor => resultEarnedStarColor;

        public Color ResultUnearnedStarColor => resultUnearnedStarColor;

        public int GetWordHintCost(int wordLength) => Mathf.Max(MinWordHintCost, wordLength);

        public int GetStarsForAssistRatio(float assistRatio)
        {
            if (assistRatio <= ThreeStarMaxAssistRatio)
                return 3;

            if (assistRatio <= TwoStarMaxAssistRatio)
                return 2;

            return 1;
        }

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

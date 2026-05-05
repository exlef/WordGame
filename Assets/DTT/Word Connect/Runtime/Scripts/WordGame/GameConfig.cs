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
        public static readonly Color DefaultGlobalBackgroundColor = new Color32(255, 192, 90, 255);
        public static readonly Color DefaultGlobalBackgroundGradientColor = new Color32(255, 173, 66, 255);
        public static readonly Color DefaultGlobalBackgroundElementsColor = new Color32(255, 204, 105, 255);
        public static readonly Color DefaultHighlightColor = new Color32(92, 130, 255, 255);
        public static readonly Color DefaultLightHighlightColor = new Color32(92, 176, 255, 255);
        public static readonly Color DefaultLetterInputLetterColor = new Color32(76, 95, 130, 255);
        public static readonly Color DefaultHintButtonColor = new Color32(246, 90, 195, 255);
        public static readonly Color DefaultLightHintButtonColor = new Color32(248, 129, 209, 255);
        public static readonly Color DefaultFoundTileColor = new Color32(69, 214, 163, 255);
        public static readonly Color DefaultTileLetterColor = Color.white;
        public static readonly Color DefaultRegularTileColor = Color.white;

        [SerializeField]
        [FormerlySerializedAs("primaryFont")]
        private Font regularFont;

        [SerializeField]
        private Font boldFont;

        [Header("Colors")]
        [SerializeField]
        private Color globalBackgroundColor = DefaultGlobalBackgroundColor;

        [SerializeField]
        private Color globalBackgroundGradientColor = DefaultGlobalBackgroundGradientColor;

        [SerializeField]
        private Color globalBackgroundElementsColor = DefaultGlobalBackgroundElementsColor;

        [SerializeField]
        private Color highlightColor = DefaultHighlightColor;

        [SerializeField]
        private Color lightHighlightColor = DefaultLightHighlightColor;

        [SerializeField]
        private Color letterInputLetterColor = DefaultLetterInputLetterColor;

        [SerializeField]
        private Color hintButtonColor = DefaultHintButtonColor;

        [SerializeField]
        private Color lightHintButtonColor = DefaultLightHintButtonColor;

        [SerializeField]
        private Color foundTileColor = DefaultFoundTileColor;

        [SerializeField]
        private Color foundTileLetterColor = DefaultTileLetterColor;

        [SerializeField]
        private Color hintedTileColor = DefaultFoundTileColor;

        [SerializeField]
        private Color hintedTileLetterColor = DefaultTileLetterColor;

        [SerializeField]
        private Color regularTileColor = DefaultRegularTileColor;

        [SerializeField]
        private LevelBackgroundColorGroup[] levelBackgroundColorGroups;

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

        public Color GlobalBackgroundColor => globalBackgroundColor;

        public Color GlobalBackgroundGradientColor => globalBackgroundGradientColor;

        public Color GlobalBackgroundElementsColor => globalBackgroundElementsColor;

        public Color HighlightColor => highlightColor;

        public Color LightHighlightColor => lightHighlightColor;

        public Color LetterInputLetterColor => letterInputLetterColor;

        public Color HintButtonColor => hintButtonColor;

        public Color LightHintButtonColor => lightHintButtonColor;

        public Color FoundTileColor => foundTileColor;

        public Color FoundTileLetterColor => foundTileLetterColor;

        public Color HintedTileColor => hintedTileColor;

        public Color HintedTileLetterColor => hintedTileLetterColor;

        public Color RegularTileColor => regularTileColor;

        public LevelBackgroundColors GlobalBackgroundColors => new LevelBackgroundColors(
            globalBackgroundColor,
            globalBackgroundGradientColor,
            globalBackgroundElementsColor);

        public int DescriptionHintCost => Mathf.Max(0, descriptionHintCost);

        public int LetterHintCost => Mathf.Max(0, letterHintCost);

        public int MinWordHintCost => Mathf.Max(0, minWordHintCost);

        public float ThreeStarMaxAssistRatio => Mathf.Clamp01(threeStarMaxAssistRatio);

        public float TwoStarMaxAssistRatio => Mathf.Clamp01(Mathf.Max(twoStarMaxAssistRatio, threeStarMaxAssistRatio));

        public Color ResultEarnedStarColor => resultEarnedStarColor;

        public Color ResultUnearnedStarColor => resultUnearnedStarColor;

        public LevelBackgroundColors GetBackgroundColorsForLevel(int levelNumber)
        {
            if (levelBackgroundColorGroups != null)
            {
                foreach (LevelBackgroundColorGroup colorGroup in levelBackgroundColorGroups)
                {
                    if (colorGroup.ContainsLevel(levelNumber))
                        return colorGroup.BackgroundColors;
                }
            }

            return GlobalBackgroundColors;
        }

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

        [System.Serializable]
        public struct LevelBackgroundColorGroup
        {
            [SerializeField]
            [Min(1)]
            private int startLevel;

            [SerializeField]
            [Min(1)]
            private int endLevel;

            [SerializeField]
            private Color backgroundColor;

            [SerializeField]
            private Color backgroundGradientColor;

            [SerializeField]
            private Color backgroundElementsColor;

            public LevelBackgroundColors BackgroundColors => new LevelBackgroundColors(
                backgroundColor,
                backgroundGradientColor,
                backgroundElementsColor);

            public bool ContainsLevel(int levelNumber)
            {
                int start = Mathf.Max(1, startLevel);
                int end = Mathf.Max(start, endLevel);
                return levelNumber >= start && levelNumber <= end;
            }
        }

        public readonly struct LevelBackgroundColors
        {
            public readonly Color BackgroundColor;

            public readonly Color BackgroundGradientColor;

            public readonly Color BackgroundElementsColor;

            public LevelBackgroundColors(Color backgroundColor, Color backgroundGradientColor, Color backgroundElementsColor)
            {
                BackgroundColor = backgroundColor;
                BackgroundGradientColor = backgroundGradientColor;
                BackgroundElementsColor = backgroundElementsColor;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using DTT.Tweening;
using WordGame;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Class that manages UI related elements for the end game results popup.
    /// </summary>
    public class ResultsUI : MonoBehaviour
    {
        /// <summary>
        /// The crossword game manager being listened to.
        /// </summary>
        private WordConnectManager _gameManager;

        /// <summary>
        /// The results UI object that should be toggled.
        /// </summary>
        [SerializeField]
        private GameObject _resultsFullUI;

        /// <summary>
        /// Legacy time text. Hidden by the assist-score result UI.
        /// </summary>
        [SerializeField]
        private Text _resultsTimeText;

        /// <summary>
        /// Text used to show the earned stars.
        /// </summary>
        [SerializeField]
        private Text _resultsTotalScoreText;

        /// <summary>
        /// Image-based star displays for the finish result card.
        /// </summary>
        [SerializeField]
        private ResultStarDisplay[] _starDisplays;

        [Header("Optional Text Fields")]
        /// <summary>
        /// The results score text to be updated.
        /// </summary>
        [SerializeField]
        private Text _resultsScoreText;

        /// <summary>
        /// The results streak score text to be updated.
        /// </summary>
        [SerializeField]
        private Text _resultsStreakScoreText;

        /// <summary>
        /// The results time score text to be updated.
        /// </summary>
        [SerializeField]
        private Text _resultsTimeScoreText;

        /// <summary>
        /// Defines if the UI should wait and listen for game results before displaying.
        /// </summary>
        [SerializeField]
        private bool _revealOnFinish = false;

        /// <summary>
        /// Defines if the UI should wait and listen for game results before displaying.
        /// </summary>
        [SerializeField]
        private bool _revealOnPause = false;

        /// <summary>
        /// The canvas group attached to the UI object to fade in or out.
        /// </summary>
        [SerializeField]
        private CanvasGroup _canvasGroup;

        /// <summary>
        /// Names used by the old result panel's text labels and score values.
        /// </summary>
        private static readonly string[] _legacyResultTextNames =
        {
            "Time",
            "Time Text",
            "Score",
            "Score Text",
            "Streak Score",
            "Streak Score Text",
            "Time Score",
            "Time Score Text",
            "Total Score Text"
        };

        /// <summary>
        /// Image and spark objects for one result star.
        /// </summary>
        [System.Serializable]
        private class ResultStarDisplay
        {
            /// <summary>
            /// Image for the star itself.
            /// </summary>
            [SerializeField]
            private Image starImage;

            /// <summary>
            /// Object containing the star spark graphics.
            /// </summary>
            [SerializeField]
            private GameObject sparkObject;

            /// <summary>
            /// Whether this display has the minimum required star image assigned.
            /// </summary>
            public bool HasStarImage => starImage != null;

            /// <summary>
            /// Applies the visual state for this star.
            /// </summary>
            /// <param name="earned">Whether the star was earned.</param>
            /// <param name="earnedColor">Color used for earned star sparks.</param>
            /// <param name="unearnedColor">Color used for unearned stars.</param>
            public void SetState(bool earned, Color earnedColor, Color unearnedColor)
            {
                if (starImage != null)
                    starImage.color = earned ? Color.white : unearnedColor;

                if (sparkObject == null)
                    return;

                sparkObject.SetActive(earned);
                if (!earned)
                    return;

                Graphic[] sparkGraphics = sparkObject.GetComponentsInChildren<Graphic>(true);
                foreach (Graphic sparkGraphic in sparkGraphics)
                    sparkGraphic.color = earnedColor;
            }
        }

        /// <summary>
        /// Subscribe to the game managers finish event if listening.
        /// </summary>
        private void OnEnable()
        {
            _gameManager = WordConnectManager.Instance;

            // Ensure UI is disabled at the start.
            _resultsFullUI.SetActive(false);
            HideLegacyResultText();

            if (_revealOnFinish)
                _gameManager.Finish += DisplayResults;
            if (_revealOnPause)
                _gameManager.Paused += DisplayPartialResults;
        }

        /// <summary>
        /// Unsubscribe from the game managers game finish event if listening.
        /// </summary>
        private void OnDisable()
        {
            if (_revealOnFinish)
                _gameManager.Finish -= DisplayResults;
            if (_revealOnPause)
                _gameManager.Paused -= DisplayPartialResults;
        }

        /// <summary>
        /// Displays the pause popup without legacy score/time values.
        /// </summary>
        private void DisplayPartialResults() => FadeInUI();

        /// <summary>
        /// Displays the results of the crossword game received.
        /// </summary>
        /// <param name="results">The game result data to be displayed.</param>
        private void DisplayResults(WordConnectResult results)
        {
            UpdateStars(results.stars);
            // Fade in the results UI after a short delay to let the last word animation complete.
            FadeInUI(1f);
        }

        /// <summary>
        /// Fades the Results UI In.
        /// </summary>
        /// <param name="delay">The delay in seconds before starting fading in the UI.</param>
        public void FadeInUI(float delay = 0)
        {
            _resultsFullUI.SetActive(true);
            DTTween.Value(0f, 1f, 0.5f, delay, Easing.EASE_OUT_EXPO, (value) => _canvasGroup.alpha = value, () => _canvasGroup.alpha = 1);
        }

        /// <summary>
        /// Fades the Results UI In.
        /// </summary>
        public void FadeOutUI() => DTTween.Value(1f, 0f, 0.5f, 0f, Easing.EASE_OUT_EXPO, (value) => _canvasGroup.alpha = value, () => _resultsFullUI.SetActive(false));


        /// <summary>
        /// Updates the result display with filled and empty star characters.
        /// </summary>
        /// <param name="stars">The amount of earned stars.</param>
        private void UpdateStars(int stars)
        {
            int clampedStars = Mathf.Clamp(stars, 0, 3);
            if (HasImageStarDisplays())
            {
                UpdateImageStars(clampedStars);
                return;
            }

            if (_resultsTotalScoreText == null)
                return;

            _resultsTotalScoreText.alignment = TextAnchor.MiddleCenter;
            _resultsTotalScoreText.text = string.Concat(
                new string('\u2605', clampedStars),
                new string('\u2606', 3 - clampedStars));
        }

        /// <summary>
        /// Updates assigned image star displays.
        /// </summary>
        /// <param name="earnedStars">The number of stars earned by the player.</param>
        private void UpdateImageStars(int earnedStars)
        {
            Color earnedColor = GetEarnedStarColor();
            Color unearnedColor = GetUnearnedStarColor();

            for (int i = 0; i < _starDisplays.Length; i++)
            {
                if (_starDisplays[i] == null)
                    continue;

                _starDisplays[i].SetState(i < earnedStars, earnedColor, unearnedColor);
            }
        }

        /// <summary>
        /// Checks whether this results UI uses image stars.
        /// </summary>
        private bool HasImageStarDisplays()
        {
            if (_starDisplays == null || _starDisplays.Length == 0)
                return false;

            foreach (ResultStarDisplay starDisplay in _starDisplays)
            {
                if (starDisplay != null && starDisplay.HasStarImage)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the configured color for earned result stars.
        /// </summary>
        private Color GetEarnedStarColor()
        {
            GameConfig gameConfig = _gameManager != null ? _gameManager.GameConfig : null;
            return gameConfig != null ? gameConfig.ResultEarnedStarColor : GameConfig.DefaultResultEarnedStarColor;
        }

        /// <summary>
        /// Gets the configured color for unearned result stars.
        /// </summary>
        private Color GetUnearnedStarColor()
        {
            GameConfig gameConfig = _gameManager != null ? _gameManager.GameConfig : null;
            return gameConfig != null ? gameConfig.ResultUnearnedStarColor : GameConfig.DefaultResultUnearnedStarColor;
        }

        /// <summary>
        /// Hides labels and fields from the old score/time result layout.
        /// </summary>
        private void HideLegacyResultText()
        {
            HideText(_resultsTimeText);
            HideText(_resultsScoreText);
            HideText(_resultsStreakScoreText);
            HideText(_resultsTimeScoreText);

            Text[] textFields = _resultsFullUI.GetComponentsInChildren<Text>(true);
            foreach (Text textField in textFields)
            {
                if (textField == _resultsTotalScoreText && _revealOnFinish)
                    continue;

                foreach (string legacyName in _legacyResultTextNames)
                {
                    if (textField.gameObject.name != legacyName)
                        continue;

                    HideText(textField);
                    break;
                }
            }
        }

        /// <summary>
        /// Hides a text field if it exists.
        /// </summary>
        /// <param name="textField">The text field to hide.</param>
        private void HideText(Text textField)
        {
            if (textField != null)
                textField.gameObject.SetActive(false);
        }
    }
}

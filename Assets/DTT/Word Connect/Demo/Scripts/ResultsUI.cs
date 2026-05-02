using System;
using UnityEngine;
using UnityEngine.UI;
using DTT.Tweening;

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
        /// The results time text to be updated.
        /// </summary>
        [SerializeField]
        private Text _resultsTimeText;

        /// <summary>
        /// The results score text to be updated.
        /// </summary>
        [SerializeField]
        private Text _resultsTotalScoreText;

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

        [Space]
        /// <summary>
        /// If the text should be set to a minutes & second format.
        /// </summary>
        [SerializeField]
        private bool _useMinuteFormatting = true;

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
        /// Subscribe to the game managers finish event if listening.
        /// </summary>
        private void OnEnable()
        {
            _gameManager = WordConnectManager.Instance;

            // Ensure UI is disabled at the start.
            _resultsFullUI.SetActive(false);

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
        /// Update the text field with the timer.
        /// </summary>
        /// <param name="totalTime">Time in seconds to be formatted.</param>
        /// <returns>The given time in mm:ss format.</returns>
        private string GetFormattedTime(float totalTime)
        {
            // Helper string to be returned after modification.
            string timeText;

            // Modify the time text based on the settings.
            if (_useMinuteFormatting)
            {
                TimeSpan currentTime = TimeSpan.FromSeconds(totalTime);
                timeText = string.Format("{0:D2}:{1:D2}", currentTime.Minutes, currentTime.Seconds);
            }
            else
            {
                timeText = ((int)totalTime).ToString();
            }

            // Return the result.
            return timeText;
        }

        /// <summary>
        /// Displays the current time elapsed and score.
        /// </summary>
        private void DisplayPartialResults()
        {
            WordConnectState state = WordConnectManager.Instance.WordConnectState;

            // Get new values and update the text.
            float timeElapsed = (float)WordConnectManager.Instance.GameTimer.Elapsed.TotalSeconds;
            string scoreEarned = (state.CurrentScore + state.CurrentStreakBonusScore).ToString();
            UpdateText(GetFormattedTime(timeElapsed), scoreEarned);
            FadeInUI();
        }

        /// <summary>
        /// Displays the results of the crossword game received.
        /// </summary>
        /// <param name="results">The game result data to be displayed.</param>
        private void DisplayResults(WordConnectResult results)
        {
            WordConnectState endState = WordConnectManager.Instance.WordConnectState;

            // Calculate all scores.
            string baseScore = endState.CurrentScore.ToString();
            string streakScore = endState.CurrentStreakBonusScore.ToString();
            string timeScore = ((int)WordConnectManager.Instance.Configuration.CalculatePoints(results.timeTaken)).ToString();
            string totalScore = results.displayScore.ToString();

            // Populate the result UI object with our scores and time data.
            UpdateText(GetFormattedTime(results.timeTaken), baseScore, streakScore, timeScore, totalScore);
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
        /// Update the total score and time text, the other score fields are optional.
        /// </summary>
        /// <param name="timeText">The time to display.</param>
        /// <param name="scoreText">The total score to display.</param>
        private void UpdateText(string timeText, string scoreText)
        {
            _resultsTimeText.text = timeText;
            _resultsTotalScoreText.text = scoreText;
        }

        /// <summary>
        /// Updates all score and time texts.
        /// </summary>
        /// <param name="timeText">The time the game has been playing for.</param>
        /// <param name="scoreText">The base score.</param>
        /// <param name="streakScoreText">The bonus score earned from a streak.</param>
        /// <param name="timeScoreText">The bonus score earned from time.</param>
        /// <param name="totalScoreText">All the previous scores added together.</param>
        private void UpdateText(string timeText, string scoreText, string streakScoreText, string timeScoreText, string totalScoreText)
        {
            _resultsTimeText.text = timeText;
            _resultsScoreText.text = scoreText;
            _resultsStreakScoreText.text = streakScoreText;
            _resultsTimeScoreText.text = timeScoreText;
            _resultsTotalScoreText.text = totalScoreText;
        }
    }
}
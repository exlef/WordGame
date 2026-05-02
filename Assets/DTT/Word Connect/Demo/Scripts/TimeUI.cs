using System;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Updates the UI to reflect the current game time 
    /// </summary>
    public class TimeUI : MonoBehaviour
    {
        /// <summary>
        /// The game manager being listened to.
        /// </summary>
        private WordConnectManager _gameManager;

        /// <summary>
        /// The text field to be updated.
        /// </summary>
        [SerializeField]
        private Text _textField;

        /// <summary>
        /// If the text should be set to a minutes & second format.
        /// </summary>
        [SerializeField]
        private bool _useMinuteFormatting = true;

        /// <summary>
        /// Get the grid manager script instance.
        /// </summary>
        private void Start() => _gameManager = WordConnectManager.Instance;

        /// <summary>
        /// Update the text each frame.
        /// </summary>
        private void Update()
        {
            if (!_gameManager.Configuration)
                return;

            UpdateText();
        }

        /// <summary>
        /// Update the text field with the timer.
        /// </summary>
        private void UpdateText()
        {
            // Declare local string.
            string timeText;

            // Modify the time text based on the settings.
            if (_useMinuteFormatting)
            {
                TimeSpan currentTime = TimeSpan.FromSeconds(_gameManager.GameTimer.Elapsed.TotalSeconds);
                timeText = string.Format("{0:D2}:{1:D2}", currentTime.Minutes, currentTime.Seconds);
            }
            else
            {
                timeText = ((int)_gameManager.GameTimer.Elapsed.TotalSeconds).ToString();
            }

            // Update the text field.
            _textField.text = timeText;
        }
    }
}
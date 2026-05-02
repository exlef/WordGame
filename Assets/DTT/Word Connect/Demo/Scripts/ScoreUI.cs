using System;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Updates the UI to reflect the current score.
    /// </summary>
    public class ScoreUI : StateUpdateUI
    {
        /// <summary>
        /// Text field which visualizes the current score.
        /// </summary>
        [SerializeField]
        private Text _textField;

        /// <summary>
        /// Sets the display text.
        /// </summary>
        /// <param name="spelledWord">The text to set.</param>
        public void SetText(string spelledWord) => _textField.text = spelledWord;

        /// <summary>
        /// Listens to the StateUpdated event on the manager script.
        /// Updates the display text to the current score.
        /// </summary>
        /// <param name="state">The updated state data.</param>
        public override void UpdateUI(WordConnectState state) => SetText((state.CurrentScore + state.CurrentStreakBonusScore).ToString());
    }
}
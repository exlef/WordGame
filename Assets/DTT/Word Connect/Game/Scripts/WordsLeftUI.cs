
using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Updates the UI to reflect how many words are left to find in the game.
    /// </summary>
    public class WordsLeftUI : StateUpdateUI
    {
        /// <summary>
        /// The textfield which will display the amount of words left to complete.
        /// </summary>
        [SerializeField]
        private Text _textField;

        /// <summary>
        /// Once a game state occurs, update the balance of letter hints and disable the button if the balance is expended.
        /// </summary>
        /// <param name="state">The class containing the updated game state data.</param>
        public override void UpdateUI(WordConnectState state) => _textField.text = (state.WordsInCrossword.Count - state.CorrectlyAddedWords.Count).ToString();
    }
}
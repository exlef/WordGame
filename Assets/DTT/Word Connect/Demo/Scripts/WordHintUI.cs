using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// This class will updates the information and disables the use of the letter hint button when necessary.
    /// </summary>
    public class WordHintUI : StateUpdateUI
    {
        /// <summary>
        /// The textfield which will be updated with the balance.
        /// </summary>
        [SerializeField]
        private Text _textField;

        /// <summary>
        /// The button object which controls the use of a word hint.
        /// </summary>
        [SerializeField]
        private Button _wordHintButton;

        /// <summary>
        /// Controls the alpha of the word hint button and all children.
        /// </summary>
        [SerializeField]
        private CanvasGroup _canvasGroup;

        /// <summary>
        /// Once a game state occurs, update the balance of word hints and disable the button if the balance is expended.
        /// </summary>
        /// <param name="state">The class containing the updated game state data.</param>
        public override void UpdateUI(WordConnectState state)
        {
            // Set the textfield to the players balance of word hints.
            _textField.text = state.WordHintBalance.ToString();

            bool hasWordHintBalance = state.WordHintBalance > 0;

            // If the player has no word hints left, disable the button.
            _wordHintButton.interactable = hasWordHintBalance;
            _canvasGroup.alpha = hasWordHintBalance ? 1f : 0.5f;
        }
    }
}
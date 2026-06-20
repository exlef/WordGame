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
        /// Icon that communicates the rewarded-ad cost of the hint.
        /// </summary>
        [SerializeField]
        private Sprite _adIconSprite;

        /// <summary>
        /// Runtime-created rewarded-ad icon.
        /// </summary>
        private Image _adIcon;

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
            // Replace the old numeric balance with a rewarded-ad cue.
            HintAdBadgeUtility.ShowAdBadge(_textField, _adIconSprite, ref _adIcon);

            bool hasWordHints = state.HintOptions.Count > 0;

            // If the player has no word hints left, disable the button.
            _wordHintButton.interactable = hasWordHints;
            _canvasGroup.alpha = hasWordHints ? 1f : 0.5f;
        }
    }
}

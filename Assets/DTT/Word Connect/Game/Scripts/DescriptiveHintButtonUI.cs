using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// This class will updates the information and disables the use of the descriptive hint button when necessary.
    /// </summary>
    public class DescriptiveHintButtonUI : StateUpdateUI
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
        /// The button object which controls the use of a descriptive hint.
        /// </summary>
        [SerializeField]
        private Button _descriptiveHintButton;

        /// <summary>
        /// Controls the alpha of the description hint button and all children.
        /// </summary>
        [SerializeField]
        private CanvasGroup _canvasGroup;

        /// <summary>
        /// Once a game state occurs, update the balance of description hints and disable the button if the balance is expended.
        /// </summary>
        /// <param name="state">The class containing the new game state data.</param>
        public override void UpdateUI(WordConnectState state)
        {
            // Replace the old numeric balance with a rewarded-ad cue.
            HintAdBadgeUtility.ShowAdBadge(_textField, _adIconSprite, ref _adIcon);

            // If a description is already open or there are no descriptions left, disable the button.
            if (state.CurrentRevealedDescriptiveHint != null || state.HintOptions.All(hint => hint.DescriptionRevealed))
            {
                _descriptiveHintButton.interactable = false;
                _canvasGroup.alpha = 0.5f;
            }
            else
            {
                _descriptiveHintButton.interactable = true;
                _canvasGroup.alpha = 1f;
            }
        }
    }
}

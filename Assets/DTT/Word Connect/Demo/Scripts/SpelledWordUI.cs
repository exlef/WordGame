using UnityEngine;
using UnityEngine.UI;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Updates the UI to reflect what word is currently spelled out.
    /// </summary>
    public class SpelledWordUI : StateUpdateUI
    {
        /// <summary>
        /// Text field which visualizes the spelled out word.
        /// </summary>
        [SerializeField]
        private Text _textField;

        /// <summary>
        /// Image component used as a background for the text.
        /// </summary>
        [SerializeField]
        private Image _background;

        /// <summary>
        /// Canvas group controls the alpha of this gameobject and its children.
        /// </summary>
        [SerializeField]
        private CanvasGroup _canvasGroup;

        /// <summary>
        /// Sets the display text.
        /// </summary>
        /// <param name="spelledWord">The text to set.</param>
        public void SetText(string spelledWord) => _textField.text = spelledWord;

        /// <summary>
        /// Sets the color of the background image.
        /// </summary>
        /// <param name="backgroundColor">The color to change to.</param>
        public void SetColor(Color backgroundColor) => _background.color = backgroundColor;

        /// <summary>
        /// On enable disable the UI.
        /// </summary>
        new private void OnEnable()
        {
            base.OnEnable();
            _canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Listens to the StateUpdated event on the manager script.
        /// Updates the display text to the current input and hides with no input.
        /// </summary>
        /// <param name="state">The updated state data.</param>
        public override void UpdateUI(WordConnectState state)
        {
            // Get the currently spelled out word.
            string spelledWord = state.GetCurrentWordInput();

            // Disable the text object as there is no input.
            _canvasGroup.alpha = string.IsNullOrWhiteSpace(spelledWord) ? 0 : 1;

            // Set the text to our spelled out word.
            SetText(spelledWord);
        }
    }
}
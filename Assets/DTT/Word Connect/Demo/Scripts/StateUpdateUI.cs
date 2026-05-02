using System;
using UnityEngine;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Abstract class which any UI script, for which its content must be updated only when the game state updates, can derive from.
    /// </summary>
    public abstract class StateUpdateUI : MonoBehaviour
    {
        /// <summary>
        /// On Start start listening to updates to the game state.
        /// </summary>
        protected void OnEnable()
        {
            WordConnectManager.Instance.StateUpdated += UpdateUI;
            WordConnectManager.Instance.Started += StateUpdated;
        }

        /// <summary>
        /// On disable stop listening to updates to the game state.
        /// </summary>
        protected void OnDisable()
        {
            WordConnectManager.Instance.StateUpdated -= UpdateUI;
            WordConnectManager.Instance.Started -= StateUpdated;
        }

        /// <summary>
        /// This function calls the UpdateUI function with a reference to the current game state data.
        /// This is necessary as the Started event does not invoke with this data attached.
        /// </summary>
        private void StateUpdated() => UpdateUI(WordConnectManager.Instance.WordConnectState);

        /// <summary>
        /// Abstract function which is called each time the game state updates.
        /// Deriving classes can implement their own functionality with the provided data.
        /// </summary>
        /// <param name="state">The updated game state data.</param>
        public abstract void UpdateUI(WordConnectState state);
    }
}

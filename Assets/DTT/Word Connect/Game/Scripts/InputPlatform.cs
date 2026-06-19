using UnityEngine;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Interface which platforms can derive from and provide functionality for pointer input.
    /// </summary>
    public abstract class InputPlatform
    {
        /// <summary>
        /// Checks based on the current input platform whether a beginning input has been registered.
        /// </summary>
        /// <returns>Bool whether the current input platform has registered a beginning input.</returns>
        public abstract bool GetPointerDown();


        /// <summary>
        /// Checks based on the current input platform whether continuous input has been registered.
        /// </summary>
        /// <returns>Bool whether the current input platform has registered a continuous input.</returns>
        public abstract bool GetPointer();

        /// <summary>
        /// Checks based on the current input platform whether ending input has been registered.
        /// </summary>
        /// <returns>Bool whether the current input platform has registered an ending input.</returns>
        public abstract bool GetPointerUp();

        /// <summary>
        /// Gets the pointer position based on the currently active input platform.
        /// </summary>
        /// <returns>The current position of the pointer on the screen.</returns>
        public abstract Vector2 GetPointerPosition();
    }
}
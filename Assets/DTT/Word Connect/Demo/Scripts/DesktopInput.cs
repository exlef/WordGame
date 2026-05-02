using UnityEngine;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// The input handler for the desktop platform.
    /// </summary>
    public class DesktopInput : InputPlatform
    {
        /// <summary>
        /// Checks whether a continuous input has been registered.
        /// </summary>
        /// <returns>Whether a continuous input has been registered.</returns>
        public override bool GetPointer() => Input.GetMouseButton(0);

        /// <summary>
        /// Checks whether a starting input has been registered.
        /// </summary>
        /// <returns>Whether a starting input has been registered.</returns>
        public override bool GetPointerDown() => Input.GetMouseButtonDown(0);

        /// <summary>
        /// Checks whether an ending input has been registered.
        /// </summary>
        /// <returns>Whether an ending input has been registered.</returns>
        public override bool GetPointerUp() => Input.GetMouseButtonUp(0);

        /// <summary>
        /// Gets the current pointer position
        /// </summary>
        /// <returns>Position of the pointer on the screen.</returns>
        public override Vector2 GetPointerPosition() => Input.mousePosition;
    }
}
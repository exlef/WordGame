using UnityEngine;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// The input handler for the mobile platform.
    /// </summary>
    public class MobileInput : InputPlatform
    {
        /// <summary>
        /// Checks whether continuous input has been registered.
        /// </summary>
        /// <returns>Whether a continuous input has been registered.</returns>
        public override bool GetPointer() => Input.touchCount > 0;

        /// <summary>
        /// Checks whether a starting input has been registered.
        /// </summary>
        /// <returns>Whether a starting input has been registered.</returns>
        public override bool GetPointerDown()
        {
            if (GetPointer())
                return Input.GetTouch(0).phase == TouchPhase.Began;
            else
                return false;
        }

        /// <summary>
        /// Checks whether an ending input has been registered.
        /// </summary>
        /// <returns>Whether an ending input has been registered.</returns>
        public override bool GetPointerUp()
        {
            if (GetPointer())
                return Input.GetTouch(0).phase == TouchPhase.Ended;
            else
                return false;
        }

        /// <summary>
        /// Gets the current pointer position
        /// </summary>
        /// <returns>Position of the pointer on the screen.</returns>
        public override Vector2 GetPointerPosition() => Input.mousePosition;
    }
}

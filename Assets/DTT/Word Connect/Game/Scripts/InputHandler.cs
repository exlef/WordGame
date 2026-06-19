using System;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// This script retrieves input data based on the active input platform.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        /// <summary>
        /// Enum for the different input platforms available.
        /// For WebGL use Desktop option.
        /// </summary>
        public enum Platform
        {
            MOBILE,
            DESKTOP
        }

        /// <summary>
        /// Current active input platform.
        /// </summary>
        public Platform inputPlatform = Platform.DESKTOP;

        /// <summary>
        /// Dictionary containing the platform enum and script controlling the input retrieval for its platform.
        /// </summary>
        private Dictionary<Platform, InputPlatform> _platformInput = new Dictionary<Platform, InputPlatform>();

        /// <summary>
        /// Event to be invoked when the input is removed from the screen
        /// </summary>
        public Action PointerUp;

        //For if the new unity input system is enabled.
#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Event to detect when the pointer is held down.
        /// </summary>
        private InputAction _pointerHeld;

        /// <summary>
        /// Whether the pointer is currently held down on the screen.
        /// </summary>
        private bool _isPointerHeld;

        /// <summary>
        /// The device used to take input data from. E.g. mouse or touchscreen.
        /// </summary>
        private InputDevice _inputDevice;

        /// <summary>
        /// Prepares the inputhandler for the new input system by initializing the input action and its events based on the current input device.
        /// </summary>
        private void InitializeInputSystemVariables()
        {
            _inputDevice = Mouse.current != null ? Mouse.current : Touchscreen.current;
            _pointerHeld = new InputAction("Pointer Held", InputActionType.Button, _inputDevice is Mouse ? "<Mouse>/leftButton" : "<Touchscreen>/primaryTouch/press");
            _pointerHeld.Enable();

            _pointerHeld.performed += ctx => _isPointerHeld = true;
            _pointerHeld.canceled += ctx =>
            {
                _isPointerHeld = false;
                PointerUp?.Invoke();
            };
        }
#endif


        #region Wrapper Methods
        /// <summary>
        /// On awake add the platform handlers to the dictionary
        /// </summary>
        private void Awake()
        {
            _platformInput.Add(Platform.MOBILE, new MobileInput());
            _platformInput.Add(Platform.DESKTOP, new DesktopInput());

#if ENABLE_INPUT_SYSTEM
            InitializeInputSystemVariables();
#endif
        }

        /// <summary>
        /// Checks based on the current input platform whether continuous input has been registered.
        /// </summary>
        /// <returns>Bool whether the current input platform has registered a continuous input.</returns>
        public bool GetPointer()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
           return GetPointerOld();
#endif
#if ENABLE_INPUT_SYSTEM
            return GetPointerNew();
#endif
        }

        /// <summary>
        /// Gets the pointer position based on the currently active input platform.
        /// </summary>
        /// <returns>The current position of the pointer on the screen.</returns>
        public Vector2 GetPointerPosition()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return GetPointerPositionOld();
#endif

#if ENABLE_INPUT_SYSTEM
            return GetPointerPositionNew();
#endif
        }
        #endregion

        #region New Input System

#if ENABLE_INPUT_SYSTEM
        public Vector2 GetPointerPositionNew()
        {
            Vector2 pointerPosition = Vector2.zero;

            if (_inputDevice is Mouse mouse)
            {
                pointerPosition = mouse.position.ReadValue();
            }
            else if (_inputDevice is Touchscreen touchscreen)
            {
                pointerPosition = touchscreen.primaryTouch.position.ReadValue();
            }

            return pointerPosition;
        }

        public bool GetPointerNew() => _isPointerHeld;

#endif
        #endregion

        #region Old Input System
#if ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// Check each frame if input is detected.
        /// </summary>
        private void Update()
        {
            if (GetPointerUpOld()) 
                PointerUp?.Invoke();
        }

        public bool GetPointerOld()
        {
            return _platformInput[inputPlatform].GetPointer();
        }
        public bool GetPointerUpOld() => _platformInput[inputPlatform].GetPointerUp();

        public Vector3 GetPointerPositionOld() => _platformInput[inputPlatform].GetPointerPosition();


#endif

        #endregion
    }


}
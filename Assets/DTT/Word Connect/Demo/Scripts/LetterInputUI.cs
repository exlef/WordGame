using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

namespace DTT.WordConnect.Demo
{
    /// <summary>
    /// Builds the UI Letter Inputs buttons and controls what happens with input.
    /// </summary>
    [RequireComponent(typeof(InputHandler))]
    public class LetterInputUI : StateUpdateUI
    {
        /// <summary>
        /// UI Object which contains our letter input buttons.
        /// </summary>
        [SerializeField]
        private RectTransform _inputContainer;

        /// <summary>
        /// The button to shuffle the letter inputs.
        /// </summary>
        [SerializeField]
        private GameObject _shuffleButton;

        /// <summary>
        /// UI Prefab which sends input for a single letter.
        /// </summary>
        [SerializeField]
        private LetterInputButton _letterInputPrefab;

        /// <summary>
        /// UI Prefab for the line connecting inputs.
        /// </summary>
        [SerializeField]
        private InputLine _inputLinePrefab;

        /// <summary>
        /// List of Letter Input GameObjects of which the UI needs to change based on the WordConnectState.
        /// </summary>
        private List<LetterInputButton> _letterInputs;

        /// <summary>
        /// List of lines which connect the highlighted inputs.
        /// </summary>
        private List<InputLine> _inputLines;

        /// <summary>
        /// Gameobject reference to the line which follows the finger on input.
        /// </summary>
        private InputLine _dynamicInputLine;

        /// <summary>
        /// The anchored position of the line which follows the finger.
        /// </summary>
        private Vector2 _dynamicLineAnchorPosition;

        /// <summary>
        /// List of positions where to draw lines from and to.
        /// </summary>
        private List<Vector2> _linePositions;
        
        /// <summary>
        /// Main camera reference to convert screen postions to to local point on canvas.
        /// </summary>
        private Camera _mainCamera;

        /// <summary>
        /// The script handeling input for different platforms.
        /// </summary>
        private InputHandler _inputHandler;

        private const float SHUFFLE_ANIMATION_DURATION = 0.5f;

        /// <summary>
        /// On awake initialize variables.
        /// </summary>
        private void Awake()
        {
            // Initialize the List variables.
            _linePositions = new List<Vector2>();
            _inputLines = new List<InputLine>();
            _letterInputs = new List<LetterInputButton>();

            _mainCamera = Camera.main;
            _inputHandler = GetComponent<InputHandler>();
        }

        /// <summary>
        /// On enable subscribe to events.
        /// </summary>
        new private void OnEnable()
        {
            // When Mouse/touch input is removed submit the spelled out word.
            _inputHandler.PointerUp += WordConnectManager.Instance.SubmitWord;

            // Once the manager has completed its initializations start the UI building.
            WordConnectManager.Instance.Started += CreateLetterInputs; 
            WordConnectManager.Instance.Started += CreateDynamicInputLine;
            WordConnectManager.Instance.StateUpdated += UpdateUI;

            //Subscribe the shuffle event to the shuffle button if it exists. 
            if (_shuffleButton.TryGetComponent(out Button button))
                button.onClick.AddListener(() => ShuffleLetterInputs(SHUFFLE_ANIMATION_DURATION));
        }

        /// <summary>
        /// On disable stop listening to game events.
        /// </summary>
        new private void OnDisable()
        {
            _inputHandler.PointerUp -= WordConnectManager.Instance.SubmitWord;
            WordConnectManager.Instance.Started -= CreateLetterInputs;
            WordConnectManager.Instance.Started -= CreateDynamicInputLine;
            WordConnectManager.Instance.StateUpdated -= UpdateUI;
        }

        /// <summary>
        /// Makes the dynamic input line ready for use.
        /// </summary>
        private void CreateDynamicInputLine()
        {
            // Clean up old dynamic line.
            if(_dynamicInputLine != null)
                Destroy(_dynamicInputLine.gameObject);

            // Destroy and clear the input lines if they exist.
            foreach (InputLine line in _inputLines)
                Destroy(line.gameObject);
            _inputLines.Clear();

            // Create the dynamic line gameObject.
            _dynamicInputLine = Instantiate(_inputLinePrefab, _inputContainer);
            _dynamicInputLine.transform.SetAsFirstSibling();
            _dynamicInputLine.UpdateColor();
            _dynamicInputLine.Disable();
        }

        /// <summary>
        /// Instantiate the Letter Input UI based on the letter which are available in the current game.
        /// </summary>
        public void CreateLetterInputs()
        {
            // Destroy and clear the letter inputs if they exist.
            foreach (LetterInputButton oldInput in _letterInputs)
                Destroy(oldInput.gameObject);
            _letterInputs.Clear();

            List<LetterInput> letters = WordConnectManager.Instance.AvailableLetters;

            // The angle between each letter input UI object.
            float angle = 360f / letters.Count;

            float containerSize = Mathf.Min(_inputContainer.rect.width, _inputContainer.rect.height);
            // Calculate the radius and input size based on the size of the container.
            float radius = containerSize / 4f;
            // Make the input size smaller based on the amount of letter inputs there are.
            float inputSize = radius - 10f * (letters.Count - 3);

            for (int i = 0; i < letters.Count; i++)
            {
                // Create the letter input object in the container.
                LetterInputButton LetterInput = Instantiate(_letterInputPrefab, _inputContainer);
                _letterInputs.Add(LetterInput);

                // Get the x and y position for the Letter Input Object with the angle and offset.
                float xPosition = radius * Mathf.Cos(Mathf.Deg2Rad * ((angle * i) + 90f));
                float yPosition = radius * Mathf.Sin(Mathf.Deg2Rad * ((angle * i) + 90f));

                // Set the UI position within the container.
                LetterInput.Rect.anchoredPosition = new Vector2(xPosition, yPosition);
                LetterInput.Rect.sizeDelta = new Vector2(inputSize, inputSize);
                // Set the text to the letter data.
                LetterInput.SetInputLetterText(letters[i].letter);
                LetterInput.UpdateColors();

                // Let our UI object listen to the PointerEnter and PointerDown event.
                // Attach the OnUserStartSelect callback with the dependant input data.
                LetterInput letterInput = letters[i];
                EventTrigger trigger = LetterInput.Trigger;

                // Callback function to handle when the letter receives input
                // The data variable must be passed on for the callback to be valid.
                Action<BaseEventData> callback = (data) => OnUserStartSelect(letterInput);
                AddEventTriggerListener(trigger, EventTriggerType.PointerEnter, callback);
                AddEventTriggerListener(trigger, EventTriggerType.PointerDown, callback);
            }
            // Shuffle the letter inputs to randomize their positions.
            ShuffleLetterInputs(0f);
        }

        /// <summary>
        /// This function is called by one of the UI Letter Inputs when a pointer enters the UI Object.
        /// </summary>
        /// <param name="letterInputData">the data which belongs to the element which was selected.</param>
        private void OnUserStartSelect(LetterInput letterInputData)
        {
            // If the shuffle animation is currently playing cancel any input.
            foreach(LetterInputButton button in _letterInputs)
                if (button.IsShuffling) 
                    return;

            // If the mouse is down or we have a touch handle the input accordingly.
            if (_inputHandler.GetPointer())
                // Send the input data to the manager which handles whether the input is allowed.
                WordConnectManager.Instance.HandleLetterInput(letterInputData);
        }

        /// <summary>
        /// Clean wrapper function which adds a callback to the specified Unity Event Trigger Type.
        /// </summary>
        /// <param name="trigger">The EventTrigger object which to attach the listener to.</param>
        /// <param name="eventType">The Unity EventTriggerType to listen to.</param>
        /// <param name="callback">The callback function to call once the Trigger gets invoked.</param>
        private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, Action<BaseEventData> callback)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback = new EventTrigger.TriggerEvent();
            entry.callback.AddListener(new UnityAction<BaseEventData>(callback));
            trigger.triggers.Add(entry);
        }

        /// <summary>
        /// Highlights the Letter Input UI objects and draws lines between highlighted inputs based on the game state data.
        /// </summary>
        /// <param name="state">Current game state data.</param>
        public override void UpdateUI(WordConnectState state)
        {
            // Disable the shuffle button if there is input.
            _shuffleButton?.SetActive(state.CurrentWordInput.Count == 0);

            // Get the currently selected letter list and the all letters list to compare which UI Letter Inputs should be in a highlighted state.
            // The allLetters list has a 1:1 index relation to the _letterInputs Gameobject list.
            List<LetterInput> selectedLetters = state.CurrentWordInput;
            List<LetterInput> allLetters = WordConnectManager.Instance.AvailableLetters;

            for (int i = 0; i < allLetters.Count; i++)
            {
                if (selectedLetters.Contains(allLetters[i]))
                {
                    // This letter input is being used, set its color to the selected state. (if it is not already in selected state)
                    if (!_letterInputs[i].IsHighlighted)
                    {
                        _letterInputs[i].IsHighlighted = true;
                        _letterInputs[i].AnimateTextColor();
                        _letterInputs[i].AnimateScale();
                    }
                }
                else
                {
                    // This letter input is not selected, set its color to the default state. (if it is not already in default state)
                    if (_letterInputs[i].IsHighlighted)
                    {
                        _letterInputs[i].IsHighlighted = false;
                        _letterInputs[i].AnimateTextColor();
                        _letterInputs[i].AnimateScale();
                    }
                }
            }
            _linePositions.Clear();
            // Get all the positions where lines need te be drawn to/from.
            for (int l = 0; l < selectedLetters.Count; l++)
            {
                int index = allLetters.IndexOf(selectedLetters[l]);
                _linePositions.Add(_letterInputs[index].Rect.anchoredPosition);
            }

            // Disable all lines.
            foreach (InputLine inputLine in _inputLines)
                inputLine.Disable();

            if (state.CurrentWordInput.Count == 0)
            {
                // No input, disable the dynamic line.
                _dynamicInputLine.Disable();
            }
            else
            {
                // Draw all active lines and activate them.
                DrawInputLines(_linePositions);
                // Set the dynamic line's anchor position to the last line position.
                _dynamicLineAnchorPosition = _linePositions[_linePositions.Count - 1];
                _dynamicInputLine.Enable();
            }
        }

        /// <summary>
        /// Enables and orients the Input lines from the object pool based on the line positions.
        /// </summary>
        /// <param name="linePositions">The positions which the lines should connect.</param>
        private void DrawInputLines(List<Vector2> linePositions)
        {
            for (int i = 0; i < linePositions.Count - 1; i++)
            {
                if (_inputLines.Count <= i)
                {
                    // If the 'object pool' does not contain enough lines, create a new one.
                    _inputLines.Add(Instantiate(_inputLinePrefab, _inputContainer));
                    // Set the object as first sibling to render underneath input buttons.
                    _inputLines[i].UpdateColor();
                    _inputLines[i].transform.SetAsFirstSibling();
                }

                Vector2 startPosition = linePositions[i];
                Vector2 endPosition = linePositions[i + 1];

                // Set the start and end position of the line.
                _inputLines[i].SetLine(startPosition, endPosition);
                _inputLines[i].Enable();
            }
        }

        /// <summary>
        /// Randomly shuffles the Letter input buttons to new positions.
        /// </summary>
        /// <param name="animationDuration">The duration of the shuffle animation in seconds.</param>
        public void ShuffleLetterInputs(float animationDuration)
        {
            // If the shuffle animation is currently playing, cancel.
            foreach (LetterInputButton button in _letterInputs)
                if (button.IsShuffling) 
                    return;

            // List of the current letter input positions.
            List<Vector2> currentPositions = GetLetterInputPositions();
            List<Vector2> newPositions = new List<Vector2>();

            Random r = new Random();

            // Create a randomized list of new positions.
            while (currentPositions.Count > 0)
            {
                //Get a random index in the current positions list.
                int randomIndex = r.Next(0, currentPositions.Count);
                // Add the random value to the new positions list.
                newPositions.Add(currentPositions[randomIndex]);
                // Remove the index in the list, so this value cant be chosen again.
                currentPositions.RemoveAt(randomIndex);
            }

            // Set the list back to the initial values.
            currentPositions = GetLetterInputPositions();

            // If the two lists are the same retry.
            if (currentPositions.SequenceEqual(newPositions))
            {
                ShuffleLetterInputs(animationDuration);
                return;
            }

            // If the the position is different, animate the letter input to the new position.
            for(int i = 0; i < currentPositions.Count; i++)
                if(currentPositions[i] != newPositions[i])
                    _letterInputs[i].AnimateShuffle(animationDuration, newPositions[i]);
        }

        /// <summary>
        /// Gets all current positions of the letter input buttons.
        /// </summary>
        /// <returns>List of letter input button positions.</returns>
        private List<Vector2> GetLetterInputPositions()
        {
            List<Vector2> positions = _letterInputs.Select((letterInput) => letterInput.Rect.anchoredPosition).ToList();
            return positions;
        }

        /// <summary>
        /// Submits the current input as soon as the mouse or touch input has been removed.
        /// Updates the dynamic input line position to the current pointer position.
        /// </summary>
        private void Update()
        {
            if (!WordConnectManager.Instance.IsGameActive) return;

            if (_dynamicLineAnchorPosition != null && _dynamicInputLine != null && _dynamicInputLine.gameObject.activeSelf)
            {
                // Calculate the end position by converting the mouse position to a local position in the parent RectTransform.
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_inputContainer, _inputHandler.GetPointerPosition(), _mainCamera, out Vector2 endPosition);
                // Set the start and end position of the line.
                _dynamicInputLine.SetLine(_dynamicLineAnchorPosition, endPosition);
            }
        }
    }
}

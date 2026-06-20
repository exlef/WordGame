using System;
using System.Collections;
using System.Collections.Generic;
using DTT.MinigameBase.LevelSelect.PaginationManagement;
using DTT.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.MinigameBase.LevelSelect
{
    /// <summary>
    /// The entry point for level selection in minigames.
    /// </summary>
    public class LevelSelect : MonoBehaviour
    {
        /// <summary>
        /// Main message shown on the final coming soon page.
        /// </summary>
        private const string COMING_SOON_TITLE = "Ast\u00EAn din z\u00FB t\u00EAn!";

        /// <summary>
        /// Supporting message shown on the final coming soon page.
        /// </summary>
        private const string COMING_SOON_SUBTITLE = "Em wan ji bo te amade dikin.";

        /// <summary>
        /// Name of the Level Select scene environment.
        /// </summary>
        public const string SCENE_NAME = "DTT Level Select";
        
        /// <summary>
        /// Called when a level is selected, passing its Level Data.
        /// </summary>
        public event Action<LevelData> LevelSelected;
        
        /// <summary>
        /// The level that is currently selected.
        /// </summary>
        public LevelButton SelectedLevel { get; private set; }
        
        /// <summary>
        /// The pagination that is used for displaying the levels in pages.
        /// </summary>
        [SerializeField]
        [Tooltip("The pagination that is used for displaying the levels in pages.")]
        private Pagination _pagination;

        /// <summary>
        /// The prefab of a button that displays a level status.
        /// </summary>
        [SerializeField]
        [Tooltip("The prefab of a button that displays a level status.")]
        private LevelButton _levelButtonPrefab;

        /// <summary>
        /// The transform parent of where the buttons reside.
        /// </summary>
        [SerializeField]
        [Tooltip("The transform parent of where the buttons reside.")]
        private Transform _buttonsParent;

        /// <summary>
        /// The database that stores all the level data.
        /// </summary>
        [SerializeField]
        [Tooltip("The database that stores all the level data.")]
        private LevelDatabase _database;

        /// <summary>
        /// Illustration displayed on the coming soon page.
        /// </summary>
        [SerializeField]
        [Tooltip("Illustration displayed on the coming soon page.")]
        private Sprite _comingSoonIllustrationSprite;

        /// <summary>
        /// All the active instances of buttons inside the level select.
        /// </summary>
        private readonly List<LevelButton> _levelButtonInstances = new List<LevelButton>();

        /// <summary>
        /// The amount of pages that contain playable level buttons.
        /// </summary>
        private int _levelPageCount;

        /// <summary>
        /// Runtime-created page that tells the player more levels are coming.
        /// </summary>
        private GameObject _comingSoonPage;

        /// <summary>
        /// Populates the Level Select environment with a given level database.
        /// </summary>
        /// <param name="database">The database for which to make the Level Select environment.</param>
        public void Populate(LevelDatabase database)
        {
            _database = database;

            _levelPageCount = Mathf.CeilToInt(_database.LevelCount / (float)15);
            _pagination.Populate(_levelPageCount + 1, 15);
            CreateComingSoonPage();
            DisplayPageLevels(_pagination.CurrentPage);
        }

        /// <summary>
        /// Adds listeners.
        /// </summary>
        private void OnEnable() => _pagination.PageChanged += DisplayPageLevels;

        /// <summary>
        /// Removes listeners.
        /// </summary>
        private void OnDisable() => _pagination.PageChanged -= DisplayPageLevels;

        /// <summary>
        /// Displays the levels for the given page number.
        /// </summary>
        /// <param name="pageNumber">The number of the page to show the levels of.</param>
        private void DisplayPageLevels(int pageNumber)
        {
            // Disable controls during transition.
            _pagination.ControlsAreEnabled = false;

            // Clean up existing levels.
            ClearLevels();

            bool showComingSoonPage = pageNumber >= _levelPageCount;
            SetComingSoonPageActive(showComingSoonPage);
            if (showComingSoonPage)
            {
                _pagination.ControlsAreEnabled = true;
                return;
            }

            // Add levels for current page.
            AddLevels();
        }

        /// <summary>
        /// Adds in the levels to the current page.
        /// </summary>
        private void AddLevels()
        {
            for (int i = 0; i < _pagination.ItemsPerPage; i++)
            {
                int dataIndex = i + _pagination.CurrentPage * _pagination.ItemsPerPage;
                if (dataIndex >= _database.LevelCount)
                {
                    _pagination.ControlsAreEnabled = true;
                    break;
                }

                LevelButton instance = Instantiate(_levelButtonPrefab, _buttonsParent);
                _levelButtonInstances.Add(instance);

                instance.transform.localScale = Vector3.zero;
                int _i = i;
                DTTween.Value(0, 1, 0.3f, i * 0.05f, Easing.EASE_OUT_BACK,
                    onValueChanged: (float value) => _levelButtonInstances[_i].transform.localScale = Vector3.one * value,
                    onComplete: () =>
                    {
                        _levelButtonInstances[_i].transform.localScale = Vector3.one;
                        if (_i == _pagination.ItemsPerPage - 1)
                            _pagination.ControlsAreEnabled = true;
                    });

                LevelData data = _database.Data[dataIndex];
                instance.name = $"Level Button - {data.levelNumber}";

                instance.PressedThis += OnLevelSelected;
                instance.SetStarsAmount(Mathf.FloorToInt(data.score * 3));
                instance.LevelNumber = data.levelNumber;
                if (data.locked)
                    instance.SetLocked();
            }
        }

        /// <summary>
        /// Clears the levels from the page.
        /// </summary>
        private void ClearLevels()
        {
            for (int i = 0; i < _levelButtonInstances.Count; i++)
            {
                LevelButton instance = _levelButtonInstances[i];
                instance.transform.SetParent(instance.transform.parent.parent);
                DTTween.Value(instance.transform.localScale.x, 0, 0.3f, 0, Easing.EASE_IN_BACK,
                    onValueChanged: (float val) => instance.transform.localScale = Vector3.one * val,
                    onComplete: () => Destroy(instance.gameObject));
            }

            _levelButtonInstances.Clear();
        }

        /// <summary>
        /// Creates the coming soon page UI.
        /// </summary>
        private void CreateComingSoonPage()
        {
            if (_comingSoonPage != null)
                return;

            Transform container = _buttonsParent.parent;
            Font font = GetComponentInChildren<Text>(true)?.font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            _comingSoonPage = new GameObject("Coming Soon Page", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _comingSoonPage.transform.SetParent(container, false);

            RectTransform pageRect = _comingSoonPage.GetComponent<RectTransform>();
            pageRect.anchorMin = new Vector2(0f, 0.18f);
            pageRect.anchorMax = new Vector2(1f, 0.78f);
            pageRect.offsetMin = Vector2.zero;
            pageRect.offsetMax = Vector2.zero;

            Image pageImage = _comingSoonPage.GetComponent<Image>();
            pageImage.color = new Color(1f, 1f, 1f, 0f);
            pageImage.raycastTarget = false;

            CreateIllustration(_comingSoonPage.transform);
            CreateMessageText(_comingSoonPage.transform, font, COMING_SOON_TITLE, 48, new Vector2(0f, -170f), new Vector2(720f, 84f), Color.white);
            CreateMessageText(_comingSoonPage.transform, font, COMING_SOON_SUBTITLE, 30, new Vector2(0f, -232f), new Vector2(720f, 70f), new Color(0.72f, 0.88f, 1f, 1f));

            _comingSoonPage.SetActive(false);
        }

        /// <summary>
        /// Creates the coming soon page illustration.
        /// </summary>
        /// <param name="parent">Parent to add the illustration to.</param>
        private void CreateIllustration(Transform parent)
        {
            GameObject illustrationObject = new GameObject("Illustration", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            illustrationObject.transform.SetParent(parent, false);

            RectTransform rect = illustrationObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 86f);
            rect.sizeDelta = new Vector2(560f, 400f);

            Image image = illustrationObject.GetComponent<Image>();
            image.sprite = _comingSoonIllustrationSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        /// <summary>
        /// Creates one line of coming soon message text.
        /// </summary>
        /// <param name="parent">Parent to add the text to.</param>
        /// <param name="font">Font to use for the text.</param>
        /// <param name="text">Text to display.</param>
        /// <param name="fontSize">Font size to use.</param>
        /// <param name="position">Anchored position of the text.</param>
        /// <param name="size">Size of the text rect.</param>
        /// <param name="color">Color of the text.</param>
        private void CreateMessageText(Transform parent, Font font, string text, int fontSize, Vector2 position, Vector2 size, Color color)
        {
            GameObject textObject = new GameObject(text, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text messageText = textObject.GetComponent<Text>();
            messageText.text = text;
            messageText.font = font;
            messageText.fontSize = fontSize;
            messageText.alignment = TextAnchor.MiddleCenter;
            messageText.color = color;
            messageText.resizeTextForBestFit = true;
            messageText.resizeTextMinSize = Mathf.Max(12, fontSize - 14);
            messageText.resizeTextMaxSize = fontSize;

            Shadow shadow = textObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0.15f, 0.35f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -3f);
        }

        /// <summary>
        /// Sets the coming soon page visibility.
        /// </summary>
        /// <param name="active">Whether the coming soon page should be visible.</param>
        private void SetComingSoonPageActive(bool active)
        {
            if (_comingSoonPage != null)
                _comingSoonPage.SetActive(active);

            _buttonsParent.gameObject.SetActive(!active);
        }

        /// <summary>
        /// Called when a level button is selected.
        /// </summary>
        /// <param name="button">The button that was clicked on.</param>
        private void OnLevelSelected(LevelButton button)
        {
            SelectedLevel = button;
            LevelSelected?.Invoke(_database.Data[button.LevelNumber - 1]);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DTT.WordConnect
{
    /// <summary>
    /// Color profile to change the looks of each game easily.
    /// </summary>
    [CreateAssetMenu(fileName = "Color Theme", menuName = "DTT/Word Connect/Color Theme")]
    public class ColorTheme : ScriptableObject
    {
        /// <summary>
        /// The color for the background.
        /// </summary>
        [field: SerializeField]
        public Color BackgroundColor { get; private set; }

        /// <summary>
        /// The color for the background gradient.
        /// </summary>
        [field: SerializeField]
        public Color BackgroundGradientColor { get; private set; }

        /// <summary>
        /// The color for the background detail elements.
        /// </summary>
        [field: SerializeField]
        public Color BackgroundElementsColor { get; private set; }

        /// <summary>
        /// The main highlight color.
        /// </summary>
        [field: SerializeField]
        public Color HighlightColor { get; private set; }

        /// <summary>
        /// A lighter version for the highlight color.
        /// </summary>
        [field: SerializeField]
        public Color LightHighlightColor { get; private set; }

        /// <summary>
        /// The color for the letter input text.
        /// </summary>
        [field: SerializeField]
        public Color LetterInputLetterColor { get; private set; }

        /// <summary>
        /// The color for the background of the hint buttons.
        /// </summary>
        [field: SerializeField]
        public Color HintButtonColor { get; private set; }

        /// <summary>
        /// The color for the background of the hint button text.
        /// </summary>
        [field: SerializeField]
        public Color LightHintButtonColor { get; private set; }

        /// <summary>
        /// The color for the found state of a tile
        /// </summary>
        [field: SerializeField]
        public Color FoundTileColor { get; private set; }

        /// <summary>
        /// The text color for the found state of a tile
        /// </summary>
        [field: SerializeField]
        public Color FoundTileLetterColor { get; private set; }

        /// <summary>
        /// The color for the hinted state of a tile
        /// </summary>
        [field: SerializeField]
        public Color HintedTileColor { get; private set; }

        /// <summary>
        /// The text color for the hinted state of a tile
        /// </summary>
        [field: SerializeField]
        public Color HintedTileLetterColor { get; private set; }

        /// <summary>
        /// The color for the regular state of a tile.
        /// </summary>
        [field: SerializeField]
        public Color RegularTileColor { get; private set; }
    }
}
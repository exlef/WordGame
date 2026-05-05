using UnityEditor;
using UnityEngine;
using DTT.PublishingTools;

namespace DTT.WordConnect.Editor
{
    [DTTHeader("dtt.word-connect", "Word Connect Configuration Data")]
    [CustomEditor(typeof(WordConnectConfigurationData), true)]
    public class WordConnectConfigurationDataEditor : DTTInspector
    {
        /// <summary>
        /// Property cache to contain all properties required for this editor script.
        /// </summary>
        ConfigurationPropertyCache propertyCache;

        /// <summary>
        /// Texture to use for the background of a tile in the layout visualization.
        /// </summary>
        public Texture2D tileTexture;

        protected override void OnEnable()
        {
            // Run the base method.
            base.OnEnable();
            propertyCache = new ConfigurationPropertyCache(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            // Run the base method.
            base.OnInspectorGUI();

            // Update the serialized object.
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            // Property fields for the game name.
            EditorGUILayout.LabelField("Game Information", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propertyCache.gameName);

            // Property fields for hints that are available in this level.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Hints", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propertyCache.letterHintsAvailable);
            EditorGUILayout.PropertyField(propertyCache.wordHintsAvailable);
            EditorGUILayout.PropertyField(propertyCache.descriptionHintsAvailable);
            EditorGUILayout.PropertyField(propertyCache.prioritizeLongestWordHint);

            // Property field for the dictionary asset.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propertyCache.dictionaryAsset);

            EditorGUILayout.Space(25);

            // Generate the visualization for the layout.
            // Check if a layout has been generated for this level.
            if (propertyCache.flattenedLetterLayout != null && propertyCache.flattenedLetterLayout.arraySize != 0)
            {
                // Check if a texture is provided for visualizing the layout tiles.
                if (tileTexture != null)
                {
                    // Calculate the size of a tile by dividing the windows width by the amount of tiles required in the width. 
                    // Subtract the tile's margin to make sure everything will fit in the window.
                    float tileSize = ((Screen.width - 50) / propertyCache.gridWidth.intValue) / 2f;
                    int margin = Mathf.CeilToInt(tileSize * 0.1f);
                    tileSize -= margin;

                    // Define the style for the empty tile GUI box.
                    var emptyStyle = new GUIStyle();
                    emptyStyle.margin = new RectOffset(margin, margin, margin, margin);
                    emptyStyle.alignment = TextAnchor.MiddleCenter;
                    emptyStyle.fontSize = Screen.width / propertyCache.gridWidth.intValue / 5;

                    // Define the style for the letter tile GUI box.
                    var tileStyle = new GUIStyle();
                    tileStyle.normal.background = tileTexture;
                    tileStyle.alignment = TextAnchor.MiddleCenter;
                    tileStyle.margin = new RectOffset(margin, margin, margin, margin);
                    tileStyle.fontSize = Screen.width / propertyCache.gridWidth.intValue / 5;

                    EditorGUILayout.BeginHorizontal();
                    // Loop over the flattened array representing the grid.
                    for (int i = 0; i < propertyCache.flattenedLetterLayout.arraySize; i++)
                    {
                        // Get the letter at this index. (replace '$' by an empty string as the influenced positions are represented by this string)
                        string gridLetter = propertyCache.flattenedLetterLayout.GetArrayElementAtIndex(i).stringValue.Replace("$", ""); ;

                        // If the tile is an empty tile draw an empty tile with default styling. ($ represents an empty tile)
                        if (string.IsNullOrEmpty(gridLetter))
                            GUILayout.Box(GUIContent.none, emptyStyle, GUILayout.Width(tileSize), GUILayout.Height(tileSize));

                        // If the tile is a letter tile draw a box with the custom styling.
                        else
                            GUILayout.Box(KurmanjiTextUtility.ToDisplayUpper(gridLetter), tileStyle, GUILayout.Width(tileSize), GUILayout.Height(tileSize));

                        // If the modulo of i is equal to 0, this means this iteration over the flattened list is on the next row of the grid.
                        // Start a new horizontal layout.
                        if (i != 0 && (i + 1) % propertyCache.gridWidth.intValue == 0)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    // There is no texture provided for visualizing the config layout.
                    // Display a warning box to provide a background texture.
                    EditorGUILayout.HelpBox("WARNING: The script has no texture to visualize the layout with. Please provide a background texture directly on the editor script itself.\nPackages > Word Connect > Demo > Scripts > WordConnectConfigurationDataEditor", MessageType.Warning);
                }
            }
            else
            {
                // No layout has been generated, show warning box.
                EditorGUILayout.HelpBox("WARNING: This ConfigurationData has no generated layout yet. Generate and save one through the WordConnectGridManager component.", MessageType.Warning);
            }

            if (EditorGUI.EndChangeCheck())
            {
                // Apply any changes made to the serialized object.
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

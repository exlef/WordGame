using UnityEditor;
using UnityEngine;
using DTT.PublishingTools;
using DTT.WordConnect.Demo;

namespace DTT.WordConnect.Editor
{
    /// <summary>
    /// Custom editor for the <see cref="WordConnectGridManager"/> that allows for editor side generation for previewing.
    /// </summary>
    [DTTHeader("dtt.word-connect", "Word Connect Grid Manager")]
    [CustomEditor(typeof(WordConnectGridManager), true)]
    public class WordConnectGridManagerEditor : DTTInspector
    {
        /// <summary>
        /// Property cache to contain all properties required for this editor script.
        /// </summary>
        GridPropertyCache propertyCache;

        /// <summary>
        /// Reference to the game data to check whether a dictionary has been attached.
        /// </summary>
        WordConnectConfigurationData gameData;


        /// <summary>
        /// Reference to the manager being modified.
        /// </summary>
        WordConnectGridManager managerReference;

        /// <summary>
        /// Find properties so that they may be displayed.
        /// </summary>
        protected override void OnEnable()
        {
            // Run the base method.
            base.OnEnable();

            // Initialize a new property cache.
            propertyCache = new GridPropertyCache(serializedObject);
            gameData = (WordConnectConfigurationData)propertyCache.gameData.objectReferenceValue;
            managerReference = (WordConnectGridManager)serializedObject.targetObject;
        }

        /// <summary>
        /// Manages the continuous displaying of GUI elements.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Run the base method.
            base.OnInspectorGUI();

            // Update the serialized object.
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            // Prefabs to use for tiles.
            EditorGUILayout.LabelField("Generation Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propertyCache.gameData);
            EditorGUILayout.PropertyField(propertyCache.letterTile);
            EditorGUILayout.PropertyField(propertyCache.blockedTile);

            // Additional fields.
            EditorGUILayout.PropertyField(propertyCache.gridLayout);
            EditorGUILayout.PropertyField(propertyCache.previewGridLayout);

            //Values for animation.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propertyCache.perWordAnimationDuration);
            EditorGUILayout.PropertyField(propertyCache.perLetterAnimationDelay);
            EditorGUILayout.PropertyField(propertyCache.animationTileScale);

            // Debug options.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propertyCache.debugAlgorithm);

            // Generation related.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generational Methods", EditorStyles.boldLabel);


            // Only show the generation button if a modifiable game data file was provided.
            if (gameData != null)
            {
                if (gameData.DictionaryAsset && gameData.DictionaryAsset.WordHintPairs.Length > 0)
                {
                    // Generate layout button.
                    if (GUILayout.Button("Generate New Layout", GUILayout.ExpandWidth(true)))
                    {
                        managerReference.GenerateNewVectorLayout();
                        managerReference.BuildActiveLayouts();
                    }

                    GUILayout.BeginHorizontal();
                    // Save layout button.
                    if (GUILayout.Button("Save Layout to Game Data", GUILayout.ExpandWidth(true)))
                        managerReference.SaveLayoutToGameData();

                    // Load layout button.
                    if (GUILayout.Button("Load Layout from Game Data", GUILayout.ExpandWidth(true)))
                    {
                        managerReference.LoadLayoutFromGameData();
                        managerReference.BuildActiveLayouts();
                    }
                    GUILayout.EndHorizontal();
                }
                // Show an error box if the game data does not have a dictionary associated with it.
                else
                {
                    EditorGUILayout.HelpBox("ERROR: Given WordConnectConfigurationData has no dictionary attached to it or has no words!", MessageType.Error);
                }
            }
            // Show a warning box if no game data file was provided.
            else
            {
                EditorGUILayout.HelpBox("WARNING: No Word Connect Configuration asset is currently provided. Either pass this through the WordConnectManager or manually set if if you want to generate and save layouts.", MessageType.Warning);
            }

            if (EditorGUI.EndChangeCheck())
            {
                // Apply any changes made to the serialized object.
                serializedObject.ApplyModifiedProperties();
                gameData = ((WordConnectConfigurationData)propertyCache.gameData.objectReferenceValue);
            }

        }
    }
}

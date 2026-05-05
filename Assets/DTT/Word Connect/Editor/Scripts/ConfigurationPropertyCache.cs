using DTT.Utils.EditorUtilities;
using UnityEditor;

public class ConfigurationPropertyCache : SerializedPropertyCache
{
    public SerializedProperty gameName => base[nameof(gameName)];
    public SerializedProperty wordHintsAvailable => base[nameof(wordHintsAvailable)];
    public SerializedProperty letterHintsAvailable => base[nameof(letterHintsAvailable)];
    public SerializedProperty descriptionHintsAvailable => base[nameof(descriptionHintsAvailable)];
    public SerializedProperty dictionaryAsset => base[nameof(dictionaryAsset)];
    public SerializedProperty wordVectors => base[nameof(wordVectors)];
    public SerializedProperty flattenedLetterLayout => base[nameof(flattenedLetterLayout)];
    public SerializedProperty gridWidth => base[nameof(gridWidth)];
    public SerializedProperty prioritizeLongestWordHint => base[nameof(prioritizeLongestWordHint)];

    public ConfigurationPropertyCache(SerializedObject serializedObject) : base(serializedObject)
    {

    }
}

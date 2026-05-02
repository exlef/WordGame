using DTT.Utils.EditorUtilities;
using UnityEditor;

public class ConfigurationPropertyCache : SerializedPropertyCache
{
    public SerializedProperty gameName => base[nameof(gameName)];
    public SerializedProperty colorTheme => base[nameof(colorTheme)];
    public SerializedProperty wordHintsAvailable => base[nameof(wordHintsAvailable)];
    public SerializedProperty letterHintsAvailable => base[nameof(letterHintsAvailable)];
    public SerializedProperty descriptionHintsAvailable => base[nameof(descriptionHintsAvailable)];
    public SerializedProperty dictionaryAsset => base[nameof(dictionaryAsset)];
    public SerializedProperty completionPointTimeCurve => base[nameof(completionPointTimeCurve)];
    public SerializedProperty expectedTimeCompletion => base[nameof(expectedTimeCompletion)];
    public SerializedProperty maxBonusTimeScore => base[nameof(maxBonusTimeScore)];
    public SerializedProperty wordVectors => base[nameof(wordVectors)];
    public SerializedProperty flattenedLetterLayout => base[nameof(flattenedLetterLayout)];
    public SerializedProperty gridWidth => base[nameof(gridWidth)];
    public SerializedProperty scorePerWordFound => base[nameof(scorePerWordFound)];
    public SerializedProperty streakScoreIncrement => base[nameof(streakScoreIncrement)];
    public SerializedProperty prioritizeLongestWordHint => base[nameof(prioritizeLongestWordHint)];

    public ConfigurationPropertyCache(SerializedObject serializedObject) : base(serializedObject)
    {

    }
}

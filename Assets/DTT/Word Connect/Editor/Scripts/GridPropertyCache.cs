using DTT.Utils.EditorUtilities;
using UnityEditor;

public class GridPropertyCache : SerializedPropertyCache
{
    public SerializedProperty letterTile => base[nameof(letterTile)];
    public SerializedProperty blockedTile => base[nameof(blockedTile)];
    public SerializedProperty gridLayout => base[nameof(gridLayout)];
    public SerializedProperty previewGridLayout => base[nameof(previewGridLayout)];
    public SerializedProperty gridViewport => base[nameof(gridViewport)];
    public SerializedProperty gridScroll => base[nameof(gridScroll)];
    public SerializedProperty gameData => base[nameof(gameData)];
    public SerializedProperty debugAlgorithm => base[nameof(debugAlgorithm)];
    public SerializedProperty perWordAnimationDuration => base[nameof(perWordAnimationDuration)];
    public SerializedProperty perLetterAnimationDelay => base[nameof(perLetterAnimationDelay)];
    public SerializedProperty animationTileScale => base[nameof(animationTileScale)];

    public GridPropertyCache(SerializedObject serializedObject) : base(serializedObject)
    {

    }
}
using UnityEditor;
public sealed class CharacterPlacementEditorWindow : ContentAssetEditorWindow<CharacterPlacementSet>
{
    [MenuItem("Under The Horizon/Content/Character Placements")]
    private static void Open() => GetWindow<CharacterPlacementEditorWindow>("Placements");
    private void OnGUI() => DrawContent();
}

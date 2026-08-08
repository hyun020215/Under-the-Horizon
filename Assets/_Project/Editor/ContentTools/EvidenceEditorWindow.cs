using UnityEditor;
public sealed class EvidenceEditorWindow : ContentAssetEditorWindow<EvidenceDefinition>
{
    [MenuItem("Under The Horizon/Content/Evidence")]
    private static void Open() => GetWindow<EvidenceEditorWindow>("Evidence");
    private void OnGUI() => DrawContent();
}

using UnityEditor;
public sealed class SequenceEditorWindow : ContentAssetEditorWindow<SceneSequenceDefinition>
{
    [MenuItem("Under The Horizon/Content/Sequences")]
    private static void Open() => GetWindow<SequenceEditorWindow>("Sequences");
    private void OnGUI() => DrawContent();
}

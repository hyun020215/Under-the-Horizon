using UnityEditor;
public sealed class InteractionEditorWindow : ContentAssetEditorWindow<InteractionSet>
{
    [MenuItem("Under The Horizon/Content/Interactions")]
    private static void Open() => GetWindow<InteractionEditorWindow>("Interactions");
    private void OnGUI() => DrawContent();
}

using UnityEditor;
public sealed class LocationEditorWindow : ContentAssetEditorWindow<LocationDefinition>
{
    [MenuItem("Under The Horizon/Content/Locations")]
    private static void Open() => GetWindow<LocationEditorWindow>("Locations");
    private void OnGUI() => DrawContent();
}

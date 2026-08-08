using UnityEditor;
public sealed class AudioCueEditorWindow : ContentAssetEditorWindow<AudioCueProfile>
{
    [MenuItem("Under The Horizon/Content/Audio Cues")]
    private static void Open() => GetWindow<AudioCueEditorWindow>("Audio Cues");
    private void OnGUI() => DrawContent();
}

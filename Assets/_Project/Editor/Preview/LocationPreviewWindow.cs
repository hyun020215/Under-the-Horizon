using UnityEditor;
using UnityEngine;
public sealed class LocationPreviewWindow : EditorWindow
{
    private LocationDefinition location;
    [MenuItem("Under The Horizon/Preview/Location")]
    private static void Open() => GetWindow<LocationPreviewWindow>("Location Preview");
    private void OnGUI()
    {
        location = (LocationDefinition)EditorGUILayout.ObjectField("Location", location, typeof(LocationDefinition), false);
        Sprite sprite = location?.DefaultBackground;
        if (sprite == null) return;
        Texture2D texture = AssetPreview.GetAssetPreview(sprite) ?? sprite.texture;
        GUI.DrawTexture(GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height), texture, ScaleMode.ScaleToFit);
    }
}

using UnityEditor;
using UnityEngine;
public sealed class CharacterPreviewWindow : EditorWindow
{
    private CharacterDefinition character;
    [MenuItem("Under The Horizon/Preview/Character")]
    private static void Open() => GetWindow<CharacterPreviewWindow>("Character Preview");
    private void OnGUI()
    {
        character = (CharacterDefinition)EditorGUILayout.ObjectField("Character", character, typeof(CharacterDefinition), false);
        Sprite sprite = character?.Portrait;
        if (sprite == null) return;
        Texture2D texture = AssetPreview.GetAssetPreview(sprite) ?? sprite.texture;
        GUI.DrawTexture(GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height), texture, ScaleMode.ScaleToFit);
    }
}

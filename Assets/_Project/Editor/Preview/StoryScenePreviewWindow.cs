using UnityEditor;
using UnityEngine;

public sealed class StoryScenePreviewWindow : EditorWindow
{
    private StorySceneDefinition scene;
    [MenuItem("Under The Horizon/Preview/Story Scene")]
    private static void Open() => GetWindow<StoryScenePreviewWindow>("Story Preview");

    private void OnGUI()
    {
        scene = (StorySceneDefinition)EditorGUILayout.ObjectField(
            "Story Scene", scene, typeof(StorySceneDefinition), false);
        if (scene == null)
        {
            EditorGUILayout.HelpBox("미리 볼 Story Scene을 선택하세요.", MessageType.Info);
            return;
        }
        EditorGUILayout.LabelField(scene.Id, scene.DisplayName, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Location", scene.Location?.DisplayName ?? "없음");
        EditorGUILayout.LabelField("Dialogue", scene.EntryDialogue?.Id ?? "없음");
        EditorGUILayout.LabelField("Characters", (scene.CharacterSet?.Placements?.Length ?? 0).ToString());
        EditorGUILayout.LabelField("Interactions", (scene.InteractionSet?.Interactions?.Length ?? 0).ToString());
        DrawSprite(scene.LocationState?.Background ?? scene.Location?.DefaultBackground);
    }

    private void DrawSprite(Sprite sprite)
    {
        if (sprite == null)
            return;
        Texture2D texture = AssetPreview.GetAssetPreview(sprite) ?? sprite.texture;
        float ratio = texture.width / (float)texture.height;
        Rect rect = GUILayoutUtility.GetAspectRect(ratio);
        GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
    }
}

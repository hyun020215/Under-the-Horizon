using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class StorySceneGraphWindow : EditorWindow
{
    private Vector2 scroll;
    [MenuItem("Under The Horizon/Content/Story Graph")]
    private static void Open() => GetWindow<StorySceneGraphWindow>("Story Graph");

    private void OnGUI()
    {
        StorySceneDefinition[] scenes = AssetDatabase.FindAssets("t:StorySceneDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<StorySceneDefinition>)
            .Where(item => item != null)
            .OrderBy(item => item.Day).ThenBy(item => item.Id)
            .ToArray();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (StorySceneDefinition scene in scenes)
        {
            string routes = scene.Routes == null || scene.Routes.Length == 0
                ? "END"
                : string.Join(", ", scene.Routes.Where(route => route != null).Select(route => route.TargetSceneId));
            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button(scene.Id, GUILayout.Width(70)))
                Selection.activeObject = scene;
            EditorGUILayout.LabelField(scene.DisplayName, GUILayout.MinWidth(180));
            EditorGUILayout.LabelField("→ " + routes);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
}

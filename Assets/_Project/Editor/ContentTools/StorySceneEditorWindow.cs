using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class ContentAssetEditorWindow<T> : EditorWindow where T : UnityEngine.Object
{
    private T[] assets = Array.Empty<T>();
    private Vector2 listScroll;
    private Vector2 inspectorScroll;
    private UnityEditor.Editor editor;
    private T selected;
    private string search = string.Empty;

    protected T SelectedAsset => selected;

    protected virtual void OnEnable() => Refresh();

    protected void DrawContent()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        search = GUILayout.TextField(search, GUI.skin.FindStyle("ToolbarSearchTextField"));
        if (GUILayout.Button("새로 고침", EditorStyles.toolbarButton, GUILayout.Width(80)))
            Refresh();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        listScroll = EditorGUILayout.BeginScrollView(listScroll, GUILayout.Width(260));
        foreach (T asset in assets.Where(MatchesSearch))
            if (GUILayout.Toggle(asset == selected, asset.name, "Button"))
                Select(asset);
        EditorGUILayout.EndScrollView();

        inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);
        if (editor != null)
        {
            editor.OnInspectorGUI();
            EditorGUILayout.Space();
            DrawSelectedTools(selected);
            if (GUILayout.Button("Project에서 선택"))
                Selection.activeObject = selected;
        }
        else
            EditorGUILayout.HelpBox("왼쪽에서 콘텐츠 자산을 선택하세요.", MessageType.Info);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    protected virtual void DrawSelectedTools(T asset)
    {
    }

    private bool MatchesSearch(T asset) => string.IsNullOrWhiteSpace(search)
        || asset.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

    private void Refresh()
    {
        assets = AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(item => item != null)
            .OrderBy(item => item.name)
            .ToArray();
        Repaint();
    }

    private void Select(T asset)
    {
        if (asset == selected)
            return;
        selected = asset;
        if (editor != null)
            DestroyImmediate(editor);
        editor = selected == null ? null : UnityEditor.Editor.CreateEditor(selected);
    }
}

public sealed class StorySceneEditorWindow : ContentAssetEditorWindow<StorySceneDefinition>
{
    [MenuItem("Under The Horizon/Content/Story Scenes")]
    private static void Open() => GetWindow<StorySceneEditorWindow>("Story Scenes");
    private void OnGUI() => DrawContent();
}

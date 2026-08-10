using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PuzzleInteractionContentMigrator
{
    private const string DefinitionRoot =
        "Assets/_Project/Content/Locations/InteractionDefinitions/Generated";
    private const string ActionRoot =
        "Assets/_Project/Content/Locations/InteractionActions/Generated";

    [MenuItem("Under The Horizon/Migration/Connect Puzzle Interactions")]
    public static void MigrateAll()
    {
        EnsureFolder(ActionRoot);

        StorySceneDefinition[] scenes = AssetDatabase
            .FindAssets("t:StorySceneDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<StorySceneDefinition>)
            .Where(scene => scene?.Puzzle != null)
            .ToArray();

        foreach (StorySceneDefinition scene in scenes)
            Connect(scene);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Connected puzzle interactions for {scenes.Length} Story Scenes.");
    }

    public static void MigrateFromCommandLine()
    {
        MigrateAll();
        EditorApplication.Exit(0);
    }

    private static void Connect(StorySceneDefinition scene)
    {
        string token = scene.Id.Replace('-', '_');
        PuzzleInteractionAction action = GetOrCreate<PuzzleInteractionAction>(
            $"{ActionRoot}/ACT_{token}_PUZZLE.asset");
        SetObject(action, "puzzle", scene.Puzzle);

        InteractionDefinition interaction = GetOrCreate<InteractionDefinition>(
            $"{DefinitionRoot}/INT_{token}_PUZZLE.asset");
        SerializedObject serialized = new(interaction);
        serialized.FindProperty("id").stringValue = $"INT_{token}_PUZZLE";
        serialized.FindProperty("type").enumValueIndex =
            (int)InteractionType.Puzzle;
        serialized.FindProperty("displayName").stringValue =
            $"{scene.DisplayName} 퍼즐";
        serialized.FindProperty("hasWorldHotspot").boolValue = true;
        serialized.FindProperty("normalizedRect").rectValue =
            new Rect(0.35f, 0.25f, 0.3f, 0.3f);
        serialized.FindProperty("action").objectReferenceValue = action;
        serialized.FindProperty("repeatable").boolValue = false;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(interaction);

        InteractionDefinition[] existing =
            scene.InteractionSet.Interactions ?? Array.Empty<InteractionDefinition>();
        InteractionDefinition[] merged = existing
            .Where(item => item != null && item.Id != interaction.Id)
            .Append(interaction)
            .ToArray();
        SetArray(scene.InteractionSet, "interactions", merged);
        UpdateRequirements(scene, merged.Length);
    }

    private static void UpdateRequirements(
        StorySceneDefinition scene,
        int interactionCount)
    {
        SerializedObject serialized = new(scene);
        SerializedProperty requirements =
            serialized.FindProperty("authoringRequirements");
        requirements.FindPropertyRelative("minimumInteractionCount").intValue =
            Math.Max(2, interactionCount);
        requirements.FindPropertyRelative("requiresPuzzle").boolValue = true;

        SerializedProperty requiredTypes =
            requirements.FindPropertyRelative("requiredInteractionTypes");
        InteractionType[] types = scene.AuthoringRequirements?
            .RequiredInteractionTypes ?? Array.Empty<InteractionType>();
        InteractionType[] merged = types
            .Append(InteractionType.Puzzle)
            .Distinct()
            .ToArray();
        requiredTypes.arraySize = merged.Length;
        for (var index = 0; index < merged.Length; index++)
            requiredTypes.GetArrayElementAtIndex(index).enumValueIndex =
                (int)merged[index];

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(scene);
    }

    private static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void SetObject(
        UnityEngine.Object target,
        string propertyName,
        UnityEngine.Object value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetArray<T>(
        UnityEngine.Object target,
        string propertyName,
        T[] values)
        where T : UnityEngine.Object
    {
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (var index = 0; index < values.Length; index++)
            property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (var index = 1; index < parts.Length; index++)
        {
            string next = $"{current}/{parts[index]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[index]);
            current = next;
        }
    }
}

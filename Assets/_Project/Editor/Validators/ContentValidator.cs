using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ContentValidator
{
    public static List<string> ValidateAll()
    {
        var errors = new List<string>();
        var ids = new HashSet<string>(System.StringComparer.Ordinal);
        foreach (string guid in AssetDatabase.FindAssets("t:StorySceneDefinition"))
        {
            var scene = AssetDatabase.LoadAssetAtPath<StorySceneDefinition>(
                AssetDatabase.GUIDToAssetPath(guid)
            );
            if (scene == null || string.IsNullOrWhiteSpace(scene.Id))
            {
                errors.Add("Story Scene has no ID: " + AssetDatabase.GUIDToAssetPath(guid));
                continue;
            }
            if (!ids.Add(scene.Id))
                errors.Add("Duplicate Story Scene ID: " + scene.Id);
            if (scene.Location == null)
                errors.Add(scene.Id + " has no Location.");
            if (scene.Routes != null)
                foreach (var route in scene.Routes)
                    if (
                        route != null
                        && !string.IsNullOrWhiteSpace(route.TargetSceneId)
                        && !Exists(route.TargetSceneId)
                    )
                        errors.Add(scene.Id + " has broken route to " + route.TargetSceneId);
        }
        return errors;
    }

    private static bool Exists(string id)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:StorySceneDefinition"))
        {
            var item = AssetDatabase.LoadAssetAtPath<StorySceneDefinition>(
                AssetDatabase.GUIDToAssetPath(guid)
            );
            if (item != null && item.Id == id)
                return true;
        }
        return false;
    }
}

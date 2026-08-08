using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ContentValidator
{
    public static List<string> ValidateAll()
    {
        var errors = new List<string>();
        StorySceneDefinition[] scenes = LoadAll<StorySceneDefinition>();
        ValidateUniqueIds(scenes, item => item.Id, "Story Scene", errors);
        ValidateUniqueIds(LoadAll<LocationDefinition>(), item => item.Id, "Location", errors);
        ValidateUniqueIds(LoadAll<CharacterDefinition>(), item => item.Id, "Character", errors);
        ValidateUniqueIds(LoadAll<DialogueSequence>(), item => item.Id, "Dialogue", errors);
        ValidateUniqueIds(LoadAll<EvidenceDefinition>(), item => item.Id, "Evidence", errors);
        ValidateUniqueIds(LoadAll<InteractionDefinition>(), item => item.Id, "Interaction", errors);

        var sceneIds = new HashSet<string>(scenes.Select(item => item.Id), StringComparer.Ordinal);
        foreach (StorySceneDefinition scene in scenes)
        {
            Require(scene, scene.Location, "Location", errors);
            Require(scene, scene.LocationState, "Location State", errors);
            Require(scene, scene.CharacterSet, "CharacterPlacementSet", errors);
            Require(scene, scene.InteractionSet, "InteractionSet", errors);
            Require(scene, scene.EntryDialogue, "Dialogue", errors);
            Require(scene, scene.AudioProfile, "Audio profile", errors);
            Require(scene, scene.EntryTransition, "entry Transition", errors);
            Require(scene, scene.ExitTransition, "exit Transition", errors);

            if (scene.CharacterSet?.Placements != null)
            {
                foreach (CharacterPlacement placement in scene.CharacterSet.Placements)
                {
                    if (placement.character == null)
                        errors.Add($"{scene.Id} has a placement without a Character.");
                    if (placement.normalizedX < 0f || placement.normalizedX > 1f
                        || placement.normalizedY < 0f || placement.normalizedY > 1f)
                        errors.Add($"{scene.Id} has a placement outside normalized bounds.");
                    if (placement.scale <= 0f)
                        errors.Add($"{scene.Id} has a placement with a non-positive scale.");
                }
            }

            if (scene.InteractionSet?.Interactions == null
                || scene.InteractionSet.Interactions.Length == 0)
                errors.Add($"{scene.Id} has no authored interactions.");
            else
                foreach (InteractionDefinition interaction in scene.InteractionSet.Interactions)
                    if (interaction == null || interaction.Action == null)
                        errors.Add($"{scene.Id} has an invalid interaction reference.");

            if (scene.OnCompleteEffects == null || scene.OnCompleteEffects.Length == 0)
                errors.Add($"{scene.Id} has no completion GameEffect.");

            if (scene.Routes == null)
                continue;
            foreach (StorySceneRoute route in scene.Routes)
                if (route != null && !string.IsNullOrWhiteSpace(route.TargetSceneId)
                    && !sceneIds.Contains(route.TargetSceneId))
                    errors.Add($"{scene.Id} has a broken route to {route.TargetSceneId}.");
        }

        foreach (LocationDefinition location in LoadAll<LocationDefinition>())
        {
            if (location.DefaultBackground == null)
                errors.Add($"{location.Id} has no default background.");
            if (location.DefaultAudio == null)
                errors.Add($"{location.Id} has no default audio profile.");
            if (location.States == null || location.States.Length == 0)
                errors.Add($"{location.Id} has no Location State.");
            else
                foreach (LocationStateDefinition state in location.States)
                    if (state == null || state.Background == null)
                        errors.Add($"{location.Id} has a State without a background.");
        }

        EvidenceDefinition[] evidence = LoadAll<EvidenceDefinition>();
        for (var number = 1; number <= 18; number++)
        {
            string id = $"C-{number:00}";
            if (!evidence.Any(item => item.Id == id))
                errors.Add($"Missing canonical Evidence {id}.");
        }

        return errors;
    }

    private static void Require(
        StorySceneDefinition scene, UnityEngine.Object value, string label, List<string> errors)
    {
        if (value == null)
            errors.Add($"{scene.Id} has no {label}.");
    }

    private static void ValidateUniqueIds<T>(
        IEnumerable<T> assets, Func<T, string> idSelector, string label, List<string> errors)
        where T : UnityEngine.Object
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (T asset in assets)
        {
            string id = idSelector(asset);
            if (string.IsNullOrWhiteSpace(id))
                errors.Add($"{label} has no ID: {AssetDatabase.GetAssetPath(asset)}");
            else if (!ids.Add(id))
                errors.Add($"Duplicate {label} ID: {id}");
        }
    }

    private static T[] LoadAll<T>() where T : UnityEngine.Object =>
        AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(item => item != null)
            .ToArray();
}

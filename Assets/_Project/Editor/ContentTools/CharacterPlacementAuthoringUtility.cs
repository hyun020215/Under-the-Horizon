using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class CharacterPlacementConversionProposal
{
    internal CharacterPlacementConversionProposal(
        CharacterPlacementSet set,
        StorySceneDefinition[] scenes,
        Sprite background,
        Vector2Int referenceResolution,
        Vector2[] before,
        Vector2[] after)
    {
        Set = set;
        Scenes = scenes;
        Background = background;
        ReferenceResolution = referenceResolution;
        Before = before;
        After = after;
    }

    public CharacterPlacementSet Set { get; }
    public IReadOnlyList<StorySceneDefinition> Scenes { get; }
    public Sprite Background { get; }
    public Vector2Int ReferenceResolution { get; }
    public IReadOnlyList<Vector2> Before { get; }
    public IReadOnlyList<Vector2> After { get; }
}

public static class CharacterPlacementAuthoringUtility
{
    private const float CoordinateTolerance = 0.00001f;

    public static StorySceneDefinition[] FindReferencingScenes(
        CharacterPlacementSet set) => AssetDatabase
        .FindAssets("t:StorySceneDefinition")
        .Select(AssetDatabase.GUIDToAssetPath)
        .Select(AssetDatabase.LoadAssetAtPath<StorySceneDefinition>)
        .Where(scene => scene != null && scene.CharacterSet == set)
        .OrderBy(scene => scene.Id, StringComparer.Ordinal)
        .ToArray();

    public static Sprite ResolveEffectiveBackground(
        StorySceneDefinition scene) => scene?.LocationState?.Background
        ?? scene?.Location?.DefaultBackground;

    public static bool TryCreateConversionProposal(
        CharacterPlacementSet set,
        IReadOnlyList<StorySceneDefinition> scenes,
        Vector2Int referenceResolution,
        IReadOnlyList<Vector2Int> requiredResolutions,
        out CharacterPlacementConversionProposal proposal,
        out string error)
    {
        proposal = null;
        error = string.Empty;
        if (set == null)
            return Fail("CharacterPlacementSet을 선택하세요.", out error);
        if (set.PlacementSpace != CharacterPlacementSpace.ViewportNormalized)
        {
            return Fail(
                "ViewportNormalized 세트만 안전 변환할 수 있습니다.",
                out error);
        }
        if (set.Placements == null || set.Placements.Length == 0)
            return Fail("빈 PlacementSet은 변환할 수 없습니다.", out error);

        StorySceneDefinition[] references = (scenes
                ?? Array.Empty<StorySceneDefinition>())
            .Where(scene => scene != null && scene.CharacterSet == set)
            .Distinct()
            .OrderBy(scene => scene.Id, StringComparer.Ordinal)
            .ToArray();
        if (references.Length == 0)
            return Fail("이 세트를 참조하는 Story Scene이 없습니다.", out error);

        Sprite[] backgrounds = references
            .Select(ResolveEffectiveBackground)
            .ToArray();
        if (backgrounds.Any(background => background == null))
        {
            return Fail(
                "참조 Story Scene 중 effective background가 없는 장면이 있습니다.",
                out error);
        }
        if (backgrounds.Distinct().Count() != 1)
        {
            return Fail(
                "같은 PlacementSet을 서로 다른 배경이 참조하므로 자동 변환할 수 없습니다.",
                out error);
        }
        if (requiredResolutions == null || requiredResolutions.Count == 0)
            return Fail("검증 해상도 행렬이 비어 있습니다.", out error);

        Sprite background = backgrounds[0];
        float aspectRatio = background.rect.width / background.rect.height;
        var viewportSize = (Vector2)referenceResolution;
        var before = new Vector2[set.Placements.Length];
        var after = new Vector2[set.Placements.Length];
        for (int index = 0; index < set.Placements.Length; index++)
        {
            CharacterPlacement placement = set.Placements[index];
            Vector2 authored = new(placement.normalizedX, placement.normalizedY);
            if (!IsNormalized(authored))
            {
                return Fail(
                    $"{placement.character?.Id ?? $"placement {index}"}의 "
                    + "좌표가 0~1 범위를 벗어났습니다.",
                    out error);
            }

            Vector2 converted;
            try
            {
                converted = WorldContentGeometry.ViewportToBackgroundNormalized(
                    authored,
                    viewportSize,
                    aspectRatio);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return Fail(exception.Message, out error);
            }

            if (!IsNormalized(converted))
            {
                return Fail(
                    $"{placement.character?.Id ?? $"placement {index}"}의 "
                    + "변환 좌표가 배경 범위를 벗어났습니다.",
                    out error);
            }

            foreach (Vector2Int resolution in requiredResolutions)
            {
                Rect visible = GetVisibleBackgroundRect(resolution, aspectRatio);
                if (!Contains(visible, converted))
                {
                    return Fail(
                        $"{placement.character?.Id ?? $"placement {index}"}의 "
                        + $"변환 좌표 {converted:F4}가 {resolution.x}×"
                        + $"{resolution.y} visible crop {visible} 밖에 있습니다. "
                        + "좌표를 먼저 안전 영역으로 재저작하세요.",
                        out error);
                }
            }

            before[index] = authored;
            after[index] = converted;
        }

        proposal = new CharacterPlacementConversionProposal(
            set,
            references,
            background,
            referenceResolution,
            before,
            after);
        return true;
    }

    public static Rect GetVisibleBackgroundRect(
        Vector2Int resolution,
        float backgroundAspectRatio)
    {
        Vector2 minimum = WorldContentGeometry.ViewportToBackgroundNormalized(
            Vector2.zero,
            resolution,
            backgroundAspectRatio);
        Vector2 maximum = WorldContentGeometry.ViewportToBackgroundNormalized(
            Vector2.one,
            resolution,
            backgroundAspectRatio);
        return Rect.MinMaxRect(
            Mathf.Min(minimum.x, maximum.x),
            Mathf.Min(minimum.y, maximum.y),
            Mathf.Max(minimum.x, maximum.x),
            Mathf.Max(minimum.y, maximum.y));
    }

    public static void ApplyConversion(
        CharacterPlacementConversionProposal proposal)
    {
        if (proposal?.Set == null)
            throw new ArgumentNullException(nameof(proposal));
        CharacterPlacementSet set = proposal.Set;
        if (set.PlacementSpace != CharacterPlacementSpace.ViewportNormalized)
        {
            throw new InvalidOperationException(
                "The placement space changed after the proposal was created.");
        }
        if (set.Placements == null
            || set.Placements.Length != proposal.Before.Count)
        {
            throw new InvalidOperationException(
                "The placements changed after the proposal was created.");
        }

        for (int index = 0; index < set.Placements.Length; index++)
        {
            Vector2 current = new(
                set.Placements[index].normalizedX,
                set.Placements[index].normalizedY);
            if ((current - proposal.Before[index]).sqrMagnitude
                > CoordinateTolerance * CoordinateTolerance)
            {
                throw new InvalidOperationException(
                    "The placement coordinates changed after the proposal "
                    + "was created.");
            }
        }

        Undo.RecordObject(set, "Convert Character Placement Space");
        SerializedObject serialized = new(set);
        serialized.FindProperty("placementSpace").intValue =
            (int)CharacterPlacementSpace.BackgroundNormalized;
        SerializedProperty placements = serialized.FindProperty("placements");
        for (int index = 0; index < placements.arraySize; index++)
        {
            SerializedProperty placement = placements.GetArrayElementAtIndex(index);
            placement.FindPropertyRelative("normalizedX").floatValue =
                proposal.After[index].x;
            placement.FindPropertyRelative("normalizedY").floatValue =
                proposal.After[index].y;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(set);
    }

    private static bool Contains(Rect rect, Vector2 point) =>
        point.x >= rect.xMin - CoordinateTolerance
        && point.x <= rect.xMax + CoordinateTolerance
        && point.y >= rect.yMin - CoordinateTolerance
        && point.y <= rect.yMax + CoordinateTolerance;

    private static bool IsNormalized(Vector2 value) =>
        value.x >= 0f && value.x <= 1f
        && value.y >= 0f && value.y <= 1f
        && !float.IsNaN(value.x) && !float.IsNaN(value.y)
        && !float.IsInfinity(value.x) && !float.IsInfinity(value.y);

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}

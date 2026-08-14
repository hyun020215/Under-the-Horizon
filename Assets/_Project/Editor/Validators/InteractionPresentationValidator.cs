using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class InteractionPresentationValidator
{
    public const string WorldHotspotPrefabPath =
        "Assets/_Project/Prefabs/Interaction/PF_Hotspot.prefab";
    public const string CharacterHotspotPrefabPath =
        "Assets/_Project/Prefabs/Interaction/PF_CharacterHotspot.prefab";
    public const string CharacterViewPrefabPath =
        "Assets/_Project/Prefabs/Characters/PF_CharacterView.prefab";
    public const string GameScenePath =
        "Assets/_Project/Scenes/Game.unity";

    private const float LayoutTolerance = 0.01f;
    private static readonly Vector2 MarkerSize = new(72f, 72f);

    public static List<string> ValidateAll()
    {
        var errors = new List<string>();
        GameObject worldHotspot = LoadPrefab(WorldHotspotPrefabPath, errors);
        GameObject characterHotspot = LoadPrefab(
            CharacterHotspotPrefabPath,
            errors);
        GameObject characterView = LoadPrefab(CharacterViewPrefabPath, errors);

        ValidateWorldHotspot(worldHotspot, WorldHotspotPrefabPath, errors);
        ValidateCharacterHotspot(
            characterHotspot,
            CharacterHotspotPrefabPath,
            errors);
        ValidateCharacterView(
            characterView,
            characterHotspot,
            CharacterViewPrefabPath,
            errors);
        ValidateGameScene(worldHotspot, errors);
        return errors;
    }

    private static GameObject LoadPrefab(
        string path,
        ICollection<string> errors)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            errors.Add($"Required interaction presentation prefab is missing: {path}.");
        return prefab;
    }

    private static void ValidateWorldHotspot(
        GameObject root,
        string label,
        ICollection<string> errors)
    {
        if (root == null)
            return;

        InteractionPointView view = root.GetComponent<InteractionPointView>();
        if (view == null)
        {
            errors.Add($"{label} must contain InteractionPointView on its root.");
            return;
        }

        Image hitSurface = root.GetComponent<Image>();
        if (hitSurface == null)
        {
            errors.Add($"{label} must contain a root Image hit surface.");
        }
        else
        {
            if (!hitSurface.enabled)
                errors.Add($"{label} root hit surface must be enabled.");
            if (!hitSurface.raycastTarget)
                errors.Add($"{label} root hit surface must receive raycasts.");
            if (hitSurface.sprite != null || hitSurface.color.a > LayoutTolerance)
            {
                errors.Add(
                    $"{label} root hit surface must be visually transparent; "
                    + "presentation belongs to the fixed Marker child.");
            }
        }

        if (!root.activeSelf)
            errors.Add($"{label} must be active in the prefab default state.");

        Transform marker = root.transform.Find("Marker");
        if (marker == null)
        {
            errors.Add($"{label} must contain a fixed-size Marker child.");
        }
        else
        {
            if (!marker.gameObject.activeSelf)
                errors.Add($"{label}/Marker must be active in the prefab default state.");

            RectTransform markerRect = marker as RectTransform;
            if (markerRect == null)
            {
                errors.Add($"{label}/Marker must use RectTransform.");
            }
            else
            {
                if (!Approximately(markerRect.anchorMin, new Vector2(0.5f, 0.5f))
                    || !Approximately(markerRect.anchorMax, new Vector2(0.5f, 0.5f)))
                {
                    errors.Add(
                        $"{label}/Marker must stay at the hit area's center "
                        + "with a point anchor so the normalized hit area cannot stretch it.");
                }
                if (!Approximately(markerRect.anchoredPosition, Vector2.zero))
                {
                    errors.Add(
                        $"{label}/Marker must remain centered with zero anchored position.");
                }
                if (!Approximately(markerRect.sizeDelta, MarkerSize))
                {
                    errors.Add(
                        $"{label}/Marker must remain {MarkerSize.x:0}x"
                        + $"{MarkerSize.y:0} logical pixels, but was "
                        + $"{markerRect.sizeDelta}.");
                }
            }

            Image markerImage = marker.GetComponent<Image>();
            if (markerImage == null)
            {
                errors.Add($"{label}/Marker must contain its visible Image.");
            }
            else
            {
                if (!markerImage.enabled)
                    errors.Add($"{label}/Marker Image must be enabled.");
                if (markerImage.sprite == null || markerImage.color.a <= LayoutTolerance)
                    errors.Add($"{label}/Marker must provide a visible affordance.");
            }

            foreach (Graphic graphic in marker.GetComponentsInChildren<Graphic>(true))
            {
                if (graphic.raycastTarget)
                {
                    errors.Add(
                        $"{label}/Marker graphic '{graphic.name}' must not "
                        + "intercept root pointer events.");
                }
            }
        }

        ValidateTooltip(root, view, label, errors);
    }

    private static void ValidateCharacterHotspot(
        GameObject root,
        string label,
        ICollection<string> errors)
    {
        if (root == null)
            return;

        InteractionPointView view = root.GetComponent<InteractionPointView>();
        if (view == null)
        {
            errors.Add($"{label} must contain InteractionPointView on its root.");
            return;
        }

        Graphic inputGraphic = root.GetComponent<Graphic>();
        if (inputGraphic == null || !inputGraphic.raycastTarget)
        {
            errors.Add(
                $"{label} visible Context affordance must receive raycasts.");
        }
        else if (!inputGraphic.enabled
                 || inputGraphic is not Image inputImage
                 || inputImage.sprite == null
                 || inputGraphic.color.a <= LayoutTolerance)
        {
            errors.Add(
                $"{label} must provide an enabled, visible Context affordance.");
        }

        RectTransform rect = root.transform as RectTransform;
        if (rect == null || !Approximately(rect.sizeDelta, MarkerSize))
        {
            errors.Add(
                $"{label} must remain a fixed {MarkerSize.x:0}x"
                + $"{MarkerSize.y:0} anchored Context affordance.");
        }
        else if (!Approximately(rect.anchorMin, rect.anchorMax))
        {
            errors.Add(
                $"{label} must use a point anchor so its fixed size cannot stretch.");
        }

        if (!root.activeSelf)
            errors.Add($"{label} must be active in the prefab default state.");

        ValidateTooltip(root, view, label, errors);
    }

    private static void ValidateTooltip(
        GameObject root,
        InteractionPointView view,
        string label,
        ICollection<string> errors)
    {
        TooltipView tooltip = view.Tooltip;
        if (tooltip == null)
        {
            errors.Add($"{label} must wire an Interaction display-name TooltipView.");
            return;
        }

        if (!tooltip.transform.IsChildOf(root.transform))
            errors.Add($"{label} TooltipView must be a child of its affordance.");
        if (tooltip.gameObject.activeSelf)
            errors.Add($"{label} TooltipView must be hidden in the prefab default state.");

        SerializedProperty labelProperty = new SerializedObject(tooltip)
            .FindProperty("label");
        if (labelProperty?.objectReferenceValue == null)
            errors.Add($"{label} TooltipView must wire its text label.");

        foreach (Graphic graphic in tooltip.GetComponentsInChildren<Graphic>(true))
        {
            if (graphic.raycastTarget)
            {
                errors.Add(
                    $"{label} Tooltip graphic '{graphic.name}' must not "
                    + "intercept interaction raycasts.");
            }
        }
    }

    private static void ValidateCharacterView(
        GameObject characterRoot,
        GameObject characterHotspotRoot,
        string label,
        ICollection<string> errors)
    {
        if (characterRoot == null || characterHotspotRoot == null)
            return;

        CharacterView character = characterRoot.GetComponent<CharacterView>();
        InteractionPointView canonicalBadge =
            characterHotspotRoot.GetComponent<InteractionPointView>();
        if (character == null)
        {
            errors.Add($"{label} must contain CharacterView on its root.");
            return;
        }
        if (canonicalBadge == null)
            return;

        SerializedProperty badgeProperty = new SerializedObject(character)
            .FindProperty("contextBadgePrefab");
        if (badgeProperty?.objectReferenceValue != canonicalBadge)
        {
            errors.Add(
                $"{label} must reference the canonical Context affordance at "
                + $"{CharacterHotspotPrefabPath}.");
        }
    }

    private static void ValidateGameScene(
        GameObject worldHotspotRoot,
        ICollection<string> errors)
    {
        if (worldHotspotRoot == null)
            return;

        InteractionPointView canonicalHotspot =
            worldHotspotRoot.GetComponent<InteractionPointView>();
        if (canonicalHotspot == null)
            return;

        Scene scene = SceneManager.GetSceneByPath(GameScenePath);
        bool openedForValidation = !scene.IsValid() || !scene.isLoaded;
        if (openedForValidation)
            scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive);

        try
        {
            InteractionDirector[] directors = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<InteractionDirector>(true))
                .ToArray();
            if (directors.Length != 1)
            {
                errors.Add(
                    $"{GameScenePath} must contain exactly one InteractionDirector, "
                    + $"but found {directors.Length}.");
                return;
            }

            SerializedObject serialized = new(directors[0]);
            SerializedProperty prefabProperty = serialized.FindProperty("hotspotPrefab");
            if (prefabProperty?.objectReferenceValue != canonicalHotspot)
            {
                errors.Add(
                    $"{GameScenePath} InteractionDirector must reference "
                    + $"{WorldHotspotPrefabPath}.");
            }

            RectTransform hotspotRoot = serialized.FindProperty("hotspotRoot")
                ?.objectReferenceValue as RectTransform;
            if (hotspotRoot == null || hotspotRoot.name != "HotspotLayer")
            {
                errors.Add(
                    $"{GameScenePath} InteractionDirector must render world "
                    + "interactions under HotspotLayer.");
            }
        }
        finally
        {
            if (openedForValidation && scene.IsValid())
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static bool Approximately(Vector2 left, Vector2 right) =>
        Mathf.Abs(left.x - right.x) <= LayoutTolerance
        && Mathf.Abs(left.y - right.y) <= LayoutTolerance;
}

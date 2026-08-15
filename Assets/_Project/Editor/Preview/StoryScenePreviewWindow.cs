using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class StoryScenePreviewWindow : EditorWindow
{
    private static readonly Vector2 ReferenceResolution = new(1920f, 1080f);
    private static readonly Vector2 CharacterReferenceSize = new(680f, 980f);
    private static readonly Vector2 ShadowReferenceSize = new(330f, 82f);
    private static readonly Vector2 ShadowReferenceOffset = new(0f, 10f);
    private static readonly Color ShadowReferenceColor =
        new(0.005f, 0.01f, 0.018f, 0.46f);

    private StorySceneDefinition scene;
    private int resolutionIndex;

    [MenuItem("Under The Horizon/Preview/Story Scene")]
    private static void Open() =>
        GetWindow<StoryScenePreviewWindow>("Story Preview");

    public static void OpenForScene(StorySceneDefinition target)
    {
        StoryScenePreviewWindow window =
            GetWindow<StoryScenePreviewWindow>("Story Preview");
        window.scene = target;
        window.Show();
        window.Focus();
        window.Repaint();
    }

    private void OnGUI()
    {
        scene = (StorySceneDefinition)EditorGUILayout.ObjectField(
            "Story Scene",
            scene,
            typeof(StorySceneDefinition),
            false);
        if (scene == null)
        {
            EditorGUILayout.HelpBox(
                "미리 볼 Story Scene을 선택하세요.",
                MessageType.Info);
            return;
        }

        Vector2Int[] resolutions = VisualQaResolutionMatrix.Resolutions.ToArray();
        resolutionIndex = EditorGUILayout.Popup(
            "Target resolution",
            Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1),
            resolutions.Select(FormatResolution).ToArray());
        Vector2Int resolution = resolutions[resolutionIndex];

        EditorGUILayout.LabelField(
            scene.Id,
            scene.DisplayName,
            EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Location",
            scene.Location?.DisplayName ?? "None");
        EditorGUILayout.LabelField(
            "Placement space",
            scene.CharacterSet?.PlacementSpace.ToString() ?? "None");
        EditorGUILayout.HelpBox(
            "이 창은 cover/crop과 캐릭터 anchor를 저작용으로 미리 봅니다. "
            + "최종 승인은 실제 Bootstrap Play Mode exact PNG로 수행하세요.",
            MessageType.Info);

        Sprite background =
            CharacterPlacementAuthoringUtility.ResolveEffectiveBackground(scene);
        if (background == null)
        {
            EditorGUILayout.HelpBox(
                "Effective background가 없습니다.",
                MessageType.Error);
            return;
        }

        Rect viewport = GUILayoutUtility.GetAspectRect(
            resolution.x / (float)resolution.y,
            GUILayout.ExpandWidth(true));
        DrawComposite(viewport, resolution, background);
    }

    private void DrawComposite(
        Rect viewport,
        Vector2Int resolution,
        Sprite background)
    {
        GUI.BeginGroup(viewport);
        Rect localViewport = new(0f, 0f, viewport.width, viewport.height);
        EditorGUI.DrawRect(localViewport, Color.black);

        float backgroundAspect = background.rect.width / background.rect.height;
        Rect pixelCover = WorldContentGeometry.CalculateCoverRect(
            resolution,
            backgroundAspect);
        float previewScale = viewport.width / resolution.x;
        Rect previewCover = new(
            pixelCover.x * previewScale,
            (resolution.y - pixelCover.y - pixelCover.height) * previewScale,
            pixelCover.width * previewScale,
            pixelCover.height * previewScale);

        Color previousColor = GUI.color;
        GUI.color = scene.LocationState != null
            ? scene.LocationState.Tint
            : Color.white;
        DrawSprite(previewCover, background);
        GUI.color = previousColor;

        CharacterPlacement[] placements =
            scene.CharacterSet?.Placements ?? System.Array.Empty<CharacterPlacement>();
        CharacterPlacement[] orderedPlacements = placements
            .OrderBy(item => item.sortingOrder)
            .ToArray();
        float canvasScale = Mathf.Sqrt(
            resolution.x / ReferenceResolution.x
            * (resolution.y / ReferenceResolution.y));
        foreach (CharacterPlacement placement in orderedPlacements)
        {
            DrawShadow(
                placement,
                scene.CharacterSet.PlacementSpace,
                resolution,
                backgroundAspect,
                previewScale,
                canvasScale);
        }
        foreach (CharacterPlacement placement in orderedPlacements)
        {
            DrawCharacter(
                placement,
                scene.CharacterSet.PlacementSpace,
                resolution,
                backgroundAspect,
                previewScale,
                canvasScale);
        }

        GUI.EndGroup();
    }

    private static void DrawShadow(
        CharacterPlacement placement,
        CharacterPlacementSpace placementSpace,
        Vector2Int resolution,
        float backgroundAspect,
        float previewScale,
        float canvasScale)
    {
        Vector2 anchorPixels = ResolveAnchorPixels(
            placement,
            placementSpace,
            resolution,
            backgroundAspect);
        float placementScale = placement.scale <= 0f ? 1f : placement.scale;
        CharacterPresentationProfile profile =
            placement.character?.PresentationOverride;
        Vector2 shadowReferenceSize = profile != null
            ? profile.GroundShadowSize
            : ShadowReferenceSize;
        Vector2 shadowReferenceOffset = profile != null
            ? profile.GroundShadowOffset
            : ShadowReferenceOffset;
        Color shadowColor = profile != null
            ? profile.GroundShadowColor
            : ShadowReferenceColor;

        Vector2 shadowSize = shadowReferenceSize
            * (canvasScale * placementScale * previewScale);
        float anchorX = anchorPixels.x * previewScale;
        float anchorY = (resolution.y - anchorPixels.y) * previewScale;
        Vector2 shadowOffset = shadowReferenceOffset
            * (canvasScale * previewScale);
        Rect shadowRect = new(
            anchorX + shadowOffset.x - shadowSize.x * 0.5f,
            anchorY - shadowOffset.y - shadowSize.y * 0.5f,
            shadowSize.x,
            shadowSize.y);
        EditorGUI.DrawRect(shadowRect, shadowColor);
    }

    private static void DrawCharacter(
        CharacterPlacement placement,
        CharacterPlacementSpace placementSpace,
        Vector2Int resolution,
        float backgroundAspect,
        float previewScale,
        float canvasScale)
    {
        Vector2 anchorPixels = ResolveAnchorPixels(
            placement,
            placementSpace,
            resolution,
            backgroundAspect);
        float placementScale = placement.scale <= 0f ? 1f : placement.scale;
        float anchorX = anchorPixels.x * previewScale;
        float anchorY = (resolution.y - anchorPixels.y) * previewScale;

        Sprite sprite = placement.character?.Resolve(
            placement.pose,
            placement.expression);
        Vector2 characterSize = CharacterReferenceSize
            * (canvasScale * placementScale * previewScale);
        Rect characterRect = new(
            anchorX - characterSize.x * 0.5f,
            anchorY - characterSize.y,
            characterSize.x,
            characterSize.y);
        if (sprite != null)
        {
            Rect fitted = ScaleToFit(characterRect, sprite.rect.size);
            DrawSprite(fitted, sprite);
        }

        const float markerSize = 6f;
        EditorGUI.DrawRect(
            new Rect(
                anchorX - markerSize * 0.5f,
                anchorY - markerSize * 0.5f,
                markerSize,
                markerSize),
            Color.yellow);
        GUI.Label(
            new Rect(anchorX + 5f, anchorY - 18f, 180f, 20f),
            placement.character?.Id ?? "Missing Character",
            EditorStyles.whiteMiniLabel);
    }

    private static Vector2 ResolveAnchorPixels(
        CharacterPlacement placement,
        CharacterPlacementSpace placementSpace,
        Vector2Int resolution,
        float backgroundAspect)
    {
        Vector2 authored = new(placement.normalizedX, placement.normalizedY);
        Vector2 viewportNormalized = placementSpace
            == CharacterPlacementSpace.BackgroundNormalized
            ? WorldContentGeometry.BackgroundToViewportNormalized(
                authored,
                resolution,
                backgroundAspect)
            : authored;
        return Vector2.Scale(viewportNormalized, resolution);
    }

    private static void DrawSprite(Rect target, Sprite sprite)
    {
        if (sprite == null || sprite.texture == null)
            return;

        Vector4 outer = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
        GUI.DrawTextureWithTexCoords(
            target,
            sprite.texture,
            Rect.MinMaxRect(outer.x, outer.y, outer.z, outer.w),
            true);
    }

    private static Rect ScaleToFit(Rect bounds, Vector2 contentSize)
    {
        if (contentSize.x <= 0f || contentSize.y <= 0f)
            return bounds;
        float scale = Mathf.Min(
            bounds.width / contentSize.x,
            bounds.height / contentSize.y);
        Vector2 size = contentSize * scale;
        return new Rect(
            bounds.center.x - size.x * 0.5f,
            bounds.yMax - size.y,
            size.x,
            size.y);
    }

    private static string FormatResolution(Vector2Int resolution) =>
        $"{resolution.x} × {resolution.y}";
}

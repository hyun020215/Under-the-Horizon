using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class CharacterPlacementAuthoringTests
{
    private const string P01PlacementPath =
        "Assets/_Project/Content/Characters/PlacementSets/Generated/"
        + "SET_P_01_CHARACTERS.asset";

    [Test]
    public void PlacementSetUsesGuardedCustomInspector()
    {
        CharacterPlacementSet set = CreateSet(new Vector2(0.5f, 0.5f));
        UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(set);

        try
        {
            Assert.That(editor, Is.TypeOf<CharacterPlacementSetInspector>());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(editor);
            DestroySet(set);
        }
    }

    [Test]
    public void P01ReferenceResolvesItsEffectiveInvitationBackground()
    {
        CharacterPlacementSet set =
            AssetDatabase.LoadAssetAtPath<CharacterPlacementSet>(P01PlacementPath);

        StorySceneDefinition[] scenes =
            CharacterPlacementAuthoringUtility.FindReferencingScenes(set);

        Assert.That(scenes.Select(scene => scene.Id), Is.EqualTo(new[] { "P-01" }));
        Sprite background =
            CharacterPlacementAuthoringUtility.ResolveEffectiveBackground(scenes[0]);
        Assert.That(background, Is.Not.Null);
        Assert.That(background.name, Is.EqualTo("BG_P01_CrumpledInvitation"));
    }

    [Test]
    public void P01PilotKeepsItsApprovedBackgroundLandmarkAcrossQaMatrix()
    {
        CharacterPlacementSet set =
            AssetDatabase.LoadAssetAtPath<CharacterPlacementSet>(P01PlacementPath);
        StorySceneDefinition[] scenes =
            CharacterPlacementAuthoringUtility.FindReferencingScenes(set);
        Sprite background =
            CharacterPlacementAuthoringUtility.ResolveEffectiveBackground(scenes[0]);
        float aspect = background.rect.width / background.rect.height;
        CharacterPlacement authored = set.Placements.Single();

        Assert.That(
            set.PlacementSpace,
            Is.EqualTo(CharacterPlacementSpace.BackgroundNormalized));
        Assert.That(authored.normalizedX, Is.EqualTo(0.6f));
        Assert.That(authored.normalizedY, Is.EqualTo(0.207f));
        Assert.That(
            authored.normalizedX * background.rect.width,
            Is.EqualTo(921.6f).Within(0.01f));
        Assert.That(
            authored.normalizedY * background.rect.height,
            Is.EqualTo(211.968f).Within(0.01f));

        foreach (Vector2Int resolution in VisualQaResolutionMatrix.Resolutions)
        {
            Vector2 viewport =
                WorldContentGeometry.BackgroundToViewportNormalized(
                    new Vector2(authored.normalizedX, authored.normalizedY),
                    resolution,
                    aspect);
            Vector2 roundTrip =
                WorldContentGeometry.ViewportToBackgroundNormalized(
                    viewport,
                    resolution,
                    aspect);
            Rect visible =
                CharacterPlacementAuthoringUtility.GetVisibleBackgroundRect(
                    resolution,
                    aspect);

            Assert.That(
                visible.Contains(new Vector2(
                    authored.normalizedX,
                    authored.normalizedY)),
                Is.True,
                $"P-01 anchor must remain visible at {resolution.x}x{resolution.y}.");
            Assert.That(
                roundTrip.x,
                Is.EqualTo(authored.normalizedX).Within(0.00001f));
            Assert.That(
                roundTrip.y,
                Is.EqualTo(authored.normalizedY).Within(0.00001f));
        }
    }

    [Test]
    public void ConverterRejectsMissingReferenceAndMixedBackgrounds()
    {
        CharacterPlacementSet set = CreateSet(new Vector2(0.5f, 0.5f));
        StorySceneDefinition first =
            ScriptableObject.CreateInstance<StorySceneDefinition>();
        StorySceneDefinition second =
            ScriptableObject.CreateInstance<StorySceneDefinition>();
        LocationStateDefinition firstState =
            ScriptableObject.CreateInstance<LocationStateDefinition>();
        LocationStateDefinition secondState =
            ScriptableObject.CreateInstance<LocationStateDefinition>();
        Sprite firstBackground = CreateSprite(150, 100);
        Sprite secondBackground = CreateSprite(160, 100);

        try
        {
            Assert.That(
                CharacterPlacementAuthoringUtility.TryCreateConversionProposal(
                    set,
                    Array.Empty<StorySceneDefinition>(),
                    new Vector2Int(1920, 1080),
                    VisualQaResolutionMatrix.Resolutions,
                    out _,
                    out string missingError),
                Is.False);
            Assert.That(missingError, Does.Contain("참조"));

            WireScene(first, set, firstState, firstBackground);
            WireScene(second, set, secondState, secondBackground);
            Assert.That(
                CharacterPlacementAuthoringUtility.TryCreateConversionProposal(
                    set,
                    new[] { first, second },
                    new Vector2Int(1920, 1080),
                    VisualQaResolutionMatrix.Resolutions,
                    out _,
                    out string mixedError),
                Is.False);
            Assert.That(mixedError, Does.Contain("서로 다른 배경"));
        }
        finally
        {
            DestroySprite(firstBackground);
            DestroySprite(secondBackground);
            UnityEngine.Object.DestroyImmediate(firstState);
            UnityEngine.Object.DestroyImmediate(secondState);
            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
            DestroySet(set);
        }
    }

    [Test]
    public void ConversionChangesOnlySpaceAndCoordinatesAndSupportsUndoRedo()
    {
        CharacterPlacementSet set = CreateSet(new Vector2(0.4f, 0.3f));
        CharacterPlacementSet control = CreateSet(new Vector2(0.7f, 0.4f));
        StorySceneDefinition scene =
            ScriptableObject.CreateInstance<StorySceneDefinition>();
        LocationStateDefinition state =
            ScriptableObject.CreateInstance<LocationStateDefinition>();
        Sprite background = CreateSprite(150, 100);

        try
        {
            WireScene(scene, set, state, background);
            CharacterPlacement before = set.Placements[0];
            CharacterPlacement controlBefore = control.Placements[0];
            Assert.That(
                CharacterPlacementAuthoringUtility.TryCreateConversionProposal(
                    set,
                    new[] { scene },
                    new Vector2Int(1920, 1080),
                    VisualQaResolutionMatrix.Resolutions,
                    out CharacterPlacementConversionProposal proposal,
                    out string error),
                Is.True,
                error);

            CharacterPlacementAuthoringUtility.ApplyConversion(proposal);
            Undo.FlushUndoRecordObjects();

            Assert.That(
                set.PlacementSpace,
                Is.EqualTo(CharacterPlacementSpace.BackgroundNormalized));
            Assert.That(set.Placements[0].normalizedX,
                Is.EqualTo(proposal.After[0].x).Within(0.00001f));
            Assert.That(set.Placements[0].normalizedY,
                Is.EqualTo(proposal.After[0].y).Within(0.00001f));
            AssertNonCoordinateFieldsEqual(before, set.Placements[0]);
            AssertPlacementEqual(controlBefore, control.Placements[0]);
            Assert.That(control.PlacementSpace,
                Is.EqualTo(CharacterPlacementSpace.ViewportNormalized));

            Undo.PerformUndo();
            Assert.That(
                set.PlacementSpace,
                Is.EqualTo(CharacterPlacementSpace.ViewportNormalized));
            AssertPlacementEqual(before, set.Placements[0]);

            Undo.PerformRedo();
            Assert.That(
                set.PlacementSpace,
                Is.EqualTo(CharacterPlacementSpace.BackgroundNormalized));
            Assert.That(set.Placements[0].normalizedY,
                Is.EqualTo(proposal.After[0].y).Within(0.00001f));
        }
        finally
        {
            Undo.ClearAll();
            DestroySprite(background);
            UnityEngine.Object.DestroyImmediate(state);
            UnityEngine.Object.DestroyImmediate(scene);
            DestroySet(set);
            DestroySet(control);
        }
    }

    [Test]
    public void VisibleBackgroundRectMatchesFiveResolutionCropContract()
    {
        Rect fhd = CharacterPlacementAuthoringUtility.GetVisibleBackgroundRect(
            new Vector2Int(1920, 1080),
            1.5f);
        Rect qhd = CharacterPlacementAuthoringUtility.GetVisibleBackgroundRect(
            new Vector2Int(2560, 1440),
            1.5f);
        Rect wuxga = CharacterPlacementAuthoringUtility.GetVisibleBackgroundRect(
            new Vector2Int(1920, 1200),
            1.5f);
        Rect ultrawide = CharacterPlacementAuthoringUtility
            .GetVisibleBackgroundRect(
                new Vector2Int(2560, 1080),
                1.5f);
        Rect ultra = CharacterPlacementAuthoringUtility.GetVisibleBackgroundRect(
            new Vector2Int(3440, 1440),
            1.5f);

        Assert.That(fhd.yMin, Is.EqualTo(0.078125f).Within(0.00001f));
        Assert.That(fhd.yMax, Is.EqualTo(0.921875f).Within(0.00001f));
        Assert.That(qhd.xMin, Is.EqualTo(fhd.xMin).Within(0.00001f));
        Assert.That(qhd.xMax, Is.EqualTo(fhd.xMax).Within(0.00001f));
        Assert.That(qhd.yMin, Is.EqualTo(fhd.yMin).Within(0.00001f));
        Assert.That(qhd.yMax, Is.EqualTo(fhd.yMax).Within(0.00001f));
        Assert.That(wuxga.yMin, Is.EqualTo(0.03125f).Within(0.00001f));
        Assert.That(wuxga.yMax, Is.EqualTo(0.96875f).Within(0.00001f));
        Assert.That(
            ultrawide.yMin,
            Is.EqualTo(0.18359375f).Within(0.00001f));
        Assert.That(
            ultrawide.yMax,
            Is.EqualTo(0.81640625f).Within(0.00001f));
        Assert.That(ultra.yMin, Is.EqualTo(0.1860465f).Within(0.00001f));
        Assert.That(ultra.yMax, Is.EqualTo(0.8139535f).Within(0.00001f));
    }

    private static CharacterPlacementSet CreateSet(Vector2 position)
    {
        CharacterPlacementSet set =
            ScriptableObject.CreateInstance<CharacterPlacementSet>();
        CharacterDefinition character =
            ScriptableObject.CreateInstance<CharacterDefinition>();
        SetPrivateField(character, "id", Guid.NewGuid().ToString("N"));
        SetPrivateField(set, "placements", new[]
        {
            new CharacterPlacement
            {
                character = character,
                normalizedX = position.x,
                normalizedY = position.y,
                scale = 0.78f,
                sortingOrder = 3,
                pose = CharacterPose.Default,
                expression = CharacterExpression.Neutral,
                clickable = true,
            },
        });
        return set;
    }

    private static void WireScene(
        StorySceneDefinition scene,
        CharacterPlacementSet set,
        LocationStateDefinition state,
        Sprite background)
    {
        SetPrivateField(state, "background", background);
        SetPrivateField(scene, "characterSet", set);
        SetPrivateField(scene, "locationState", state);
    }

    private static Sprite CreateSprite(int width, int height)
    {
        Texture2D texture = new(width, height);
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f));
    }

    private static void DestroySprite(Sprite sprite)
    {
        Texture2D texture = sprite.texture;
        UnityEngine.Object.DestroyImmediate(sprite);
        UnityEngine.Object.DestroyImmediate(texture);
    }

    private static void DestroySet(CharacterPlacementSet set)
    {
        CharacterDefinition[] characters = (set.Placements
                ?? Array.Empty<CharacterPlacement>())
            .Select(placement => placement.character)
            .Where(character => character != null)
            .Distinct()
            .ToArray();
        UnityEngine.Object.DestroyImmediate(set);
        foreach (CharacterDefinition character in characters)
            UnityEngine.Object.DestroyImmediate(character);
    }

    private static void AssertNonCoordinateFieldsEqual(
        CharacterPlacement expected,
        CharacterPlacement actual)
    {
        Assert.That(actual.character, Is.SameAs(expected.character));
        Assert.That(actual.scale, Is.EqualTo(expected.scale));
        Assert.That(actual.sortingOrder, Is.EqualTo(expected.sortingOrder));
        Assert.That(actual.pose, Is.EqualTo(expected.pose));
        Assert.That(actual.expression, Is.EqualTo(expected.expression));
        Assert.That(actual.clickable, Is.EqualTo(expected.clickable));
    }

    private static void AssertPlacementEqual(
        CharacterPlacement expected,
        CharacterPlacement actual)
    {
        AssertNonCoordinateFieldsEqual(expected, actual);
        Assert.That(actual.normalizedX, Is.EqualTo(expected.normalizedX));
        Assert.That(actual.normalizedY, Is.EqualTo(expected.normalizedY));
    }

    private static void SetPrivateField(
        object target,
        string name,
        object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
        ?.SetValue(target, value);
}

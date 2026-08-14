using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class InteractionPresentationTests
{
    private const string InvitationPath =
        "Assets/_Project/Content/Locations/InteractionDefinitions/Generated/"
        + "INT_P_01_INVITATION.asset";
    private const string LargePuzzlePath =
        "Assets/_Project/Content/Locations/InteractionDefinitions/Generated/"
        + "INT_D2_02_PUZZLE.asset";
    private static readonly Vector2 ExpectedMarkerSize = new(72f, 72f);

    [Test]
    public void CanonicalInteractionPresentationPassesBuildPreflightRules()
    {
        var errors = InteractionPresentationValidator.ValidateAll();
        Assert.That(errors, Is.Empty, string.Join("\n", errors));
    }

    [Test]
    public void CanonicalWorldAndCharacterHotspotsPreserveTheirDistinctContracts()
    {
        GameObject world = AssetDatabase.LoadAssetAtPath<GameObject>(
            InteractionPresentationValidator.WorldHotspotPrefabPath);
        GameObject character = AssetDatabase.LoadAssetAtPath<GameObject>(
            InteractionPresentationValidator.CharacterHotspotPrefabPath);

        AssertWorldHotspotContract(world);
        AssertCharacterHotspotContract(character);
    }

    [Test]
    public void CharacterViewAndGameSceneReferenceCanonicalInteractionPrefabs()
    {
        GameObject world = AssetDatabase.LoadAssetAtPath<GameObject>(
            InteractionPresentationValidator.WorldHotspotPrefabPath);
        GameObject characterHotspot = AssetDatabase.LoadAssetAtPath<GameObject>(
            InteractionPresentationValidator.CharacterHotspotPrefabPath);
        GameObject characterRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
            InteractionPresentationValidator.CharacterViewPrefabPath);
        Assert.That(world, Is.Not.Null);
        Assert.That(characterHotspot, Is.Not.Null);
        Assert.That(characterRoot, Is.Not.Null);

        InteractionPointView canonicalWorld =
            world.GetComponent<InteractionPointView>();
        InteractionPointView canonicalCharacter =
            characterHotspot.GetComponent<InteractionPointView>();
        CharacterView character = characterRoot.GetComponent<CharacterView>();
        Assert.That(canonicalWorld, Is.Not.Null);
        Assert.That(canonicalCharacter, Is.Not.Null);
        Assert.That(character, Is.Not.Null);
        Assert.That(
            new SerializedObject(character)
                .FindProperty("contextBadgePrefab")
                .objectReferenceValue,
            Is.SameAs(canonicalCharacter));

        Scene scene = SceneManager.GetSceneByPath(
            InteractionPresentationValidator.GameScenePath);
        bool openedForTest = !scene.IsValid() || !scene.isLoaded;
        if (openedForTest)
        {
            scene = EditorSceneManager.OpenScene(
                InteractionPresentationValidator.GameScenePath,
                OpenSceneMode.Additive);
        }

        try
        {
            InteractionDirector director = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<InteractionDirector>(true))
                .Single();
            SerializedObject serialized = new(director);
            Assert.That(
                serialized.FindProperty("hotspotPrefab").objectReferenceValue,
                Is.SameAs(canonicalWorld));
            Assert.That(
                serialized.FindProperty("hotspotRoot")
                    .objectReferenceValue.name,
                Is.EqualTo("HotspotLayer"));
        }
        finally
        {
            if (openedForTest && scene.IsValid())
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void P0BuilderFactoriesProduceCanonicalInteractionAffordances()
    {
        Type builder = typeof(P0ProjectBuilder);
        MethodInfo build = builder.GetMethod(
            "BuildInteractionPrefabs",
            BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo worldFactory = builder.GetMethod(
            "CreateWorldHotspotPrefab",
            BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo characterFactory = builder.GetMethod(
            "CreateCharacterHotspotPrefab",
            BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo refresh = builder.GetMethod(
            "RefreshInteractionPrefabsFromCommandLine",
            BindingFlags.Public | BindingFlags.Static);

        Assert.That(build, Is.Not.Null);
        Assert.That(worldFactory, Is.Not.Null);
        Assert.That(characterFactory, Is.Not.Null);
        Assert.That(refresh, Is.Not.Null);

        GameObject world = null;
        GameObject character = null;
        try
        {
            world = worldFactory.Invoke(
                null,
                new object[] { "PF_Hotspot_FactoryTest" }) as GameObject;
            character = characterFactory.Invoke(
                null,
                new object[] { "PF_CharacterHotspot_FactoryTest" }) as GameObject;

            AssertWorldHotspotContract(world);
            AssertCharacterHotspotContract(character);
        }
        finally
        {
            if (world != null)
                UnityEngine.Object.DestroyImmediate(world);
            if (character != null)
                UnityEngine.Object.DestroyImmediate(character);
        }
    }

    [Test]
    public void NormalizedHitAreaNeverStretchesTheFixedWorldMarker()
    {
        InteractionDefinition invitation =
            AssetDatabase.LoadAssetAtPath<InteractionDefinition>(InvitationPath);
        InteractionDefinition largePuzzle =
            AssetDatabase.LoadAssetAtPath<InteractionDefinition>(LargePuzzlePath);
        Assert.That(invitation, Is.Not.Null);
        Assert.That(largePuzzle, Is.Not.Null);
        Assert.That(largePuzzle.NormalizedRect.width, Is.EqualTo(0.3f));
        Assert.That(largePuzzle.NormalizedRect.height, Is.EqualTo(0.3f));

        MethodInfo factory = typeof(P0ProjectBuilder).GetMethod(
            "CreateWorldHotspotPrefab",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(factory, Is.Not.Null);
        GameObject root = factory.Invoke(
            null,
            new object[] { "PF_Hotspot_LayoutTest" }) as GameObject;

        try
        {
            Assert.That(root, Is.Not.Null);
            InteractionPointView view = root.GetComponent<InteractionPointView>();
            RectTransform rootRect = root.transform as RectTransform;
            RectTransform marker = root.transform.Find("Marker") as RectTransform;
            Assert.That(view, Is.Not.Null);
            Assert.That(rootRect, Is.Not.Null);
            Assert.That(marker, Is.Not.Null);

            view.Apply(invitation);
            Assert.That(rootRect.anchorMin, Is.EqualTo(invitation.NormalizedRect.min));
            Assert.That(rootRect.anchorMax, Is.EqualTo(invitation.NormalizedRect.max));
            Assert.That(marker.sizeDelta, Is.EqualTo(ExpectedMarkerSize));
            Assert.That(marker.anchorMin, Is.EqualTo(marker.anchorMax));

            view.Apply(largePuzzle);
            Assert.That(rootRect.anchorMin, Is.EqualTo(largePuzzle.NormalizedRect.min));
            Assert.That(rootRect.anchorMax, Is.EqualTo(largePuzzle.NormalizedRect.max));
            Assert.That(marker.sizeDelta, Is.EqualTo(ExpectedMarkerSize));
            Assert.That(marker.anchorMin, Is.EqualTo(marker.anchorMax));
        }
        finally
        {
            if (root != null)
                UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void AuthoredWorldHotspotsProvideTooltipDisplayNames()
    {
        string[] missing = AssetDatabase
            .FindAssets(
                "t:InteractionDefinition",
                new[] { "Assets/_Project/Content" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<InteractionDefinition>)
            .Where(definition => definition != null
                && definition.HasWorldHotspot
                && string.IsNullOrWhiteSpace(definition.DisplayName))
            .Select(definition => definition.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();

        Assert.That(
            missing,
            Is.Empty,
            "World hotspot tooltips require authored display names.");
    }

    private static void AssertWorldHotspotContract(GameObject root)
    {
        Assert.That(root, Is.Not.Null);
        InteractionPointView view = root.GetComponent<InteractionPointView>();
        Image hitSurface = root.GetComponent<Image>();
        RectTransform marker = root.transform.Find("Marker") as RectTransform;

        Assert.That(view, Is.Not.Null);
        Assert.That(hitSurface, Is.Not.Null);
        Assert.That(root.activeSelf, Is.True);
        Assert.That(hitSurface.enabled, Is.True);
        Assert.That(hitSurface.raycastTarget, Is.True);
        Assert.That(hitSurface.sprite, Is.Null);
        Assert.That(hitSurface.color.a, Is.EqualTo(0f).Within(0.001f));
        Assert.That(marker, Is.Not.Null);
        Assert.That(marker.gameObject.activeSelf, Is.True);
        Assert.That(marker.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(marker.anchorMax, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(marker.anchoredPosition, Is.EqualTo(Vector2.zero));
        Assert.That(marker.sizeDelta, Is.EqualTo(ExpectedMarkerSize));
        Assert.That(marker.GetComponent<Image>(), Is.Not.Null);
        Assert.That(marker.GetComponent<Image>().sprite, Is.Not.Null);
        Assert.That(marker.GetComponent<Image>().enabled, Is.True);
        Assert.That(
            marker.GetComponentsInChildren<Graphic>(true)
                .All(graphic => !graphic.raycastTarget),
            Is.True,
            "Marker graphics must leave pointer handling on the transparent root.");
        AssertTooltipContract(root, view);
    }

    private static void AssertCharacterHotspotContract(GameObject root)
    {
        Assert.That(root, Is.Not.Null);
        InteractionPointView view = root.GetComponent<InteractionPointView>();
        Graphic inputGraphic = root.GetComponent<Graphic>();
        RectTransform rect = root.transform as RectTransform;

        Assert.That(view, Is.Not.Null);
        Assert.That(inputGraphic, Is.Not.Null);
        Assert.That(root.activeSelf, Is.True);
        Assert.That(inputGraphic.enabled, Is.True);
        Assert.That(inputGraphic.color.a, Is.GreaterThan(0f));
        Assert.That(inputGraphic, Is.TypeOf<Image>());
        Assert.That(((Image)inputGraphic).sprite, Is.Not.Null);
        Assert.That(inputGraphic.raycastTarget, Is.True);
        Assert.That(rect, Is.Not.Null);
        Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax));
        Assert.That(rect.sizeDelta, Is.EqualTo(ExpectedMarkerSize));
        AssertTooltipContract(root, view);
    }

    private static void AssertTooltipContract(
        GameObject root,
        InteractionPointView view)
    {
        TooltipView tooltip = view.Tooltip;
        Assert.That(tooltip, Is.Not.Null);
        Assert.That(tooltip.transform.IsChildOf(root.transform), Is.True);
        Assert.That(tooltip.gameObject.activeSelf, Is.False);
        Assert.That(
            new SerializedObject(tooltip)
                .FindProperty("label")
                .objectReferenceValue,
            Is.Not.Null);
        Assert.That(
            tooltip.GetComponentsInChildren<Graphic>(true)
                .All(graphic => !graphic.raycastTarget),
            Is.True,
            "Tooltip graphics must not steal pointer events from the affordance root.");
    }
}

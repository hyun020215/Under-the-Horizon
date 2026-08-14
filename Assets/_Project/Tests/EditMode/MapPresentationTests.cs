using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapPresentationTests
{
    [Test]
    public void MapPrefabUsesAuthoredDeckDefinitionsAndLayerViews()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(
            "Assets/_Project/Prefabs/UI/PF_MapScreen.prefab");
        try
        {
            MapScreen screen = root.GetComponent<MapScreen>();
            Assert.That(screen, Is.Not.Null);

            Transform viewport = root.transform.Find("Map Viewport");
            Transform surface = viewport?.Find("Map Surface");
            Transform baseLayer = surface?.Find("Base Layer");
            Transform restrictedLayer = surface?.Find("Restricted Layer");
            Transform technicalLayer = surface?.Find("Technical Layer");
            Transform nodeRoot = surface?.Find("Node Root");
            Transform nodeTemplate = nodeRoot?.Find("Location Node Template");
            Transform details = root.transform.Find("Location Details");

            Assert.That(viewport, Is.Not.Null);
            Assert.That(surface, Is.Not.Null);
            Assert.That(baseLayer, Is.Not.Null);
            Assert.That(restrictedLayer, Is.Not.Null);
            Assert.That(technicalLayer, Is.Not.Null);
            Assert.That(nodeRoot, Is.Not.Null);
            Assert.That(nodeTemplate, Is.Not.Null);
            Assert.That(nodeTemplate.gameObject.activeSelf, Is.False);
            Assert.That(details?.Find("Selection Name"), Is.Not.Null);
            Assert.That(details?.Find("Selection Status"), Is.Not.Null);
            Assert.That(details?.Find("Selection Description"), Is.Not.Null);
            Assert.That(details?.Find("Travel Feedback"), Is.Not.Null);
            Assert.That(details?.Find("Confirm Travel Button"), Is.Not.Null);

            AspectRatioFitter aspect = surface.GetComponent<AspectRatioFitter>();
            Assert.That(aspect, Is.Not.Null);
            Assert.That(aspect.aspectMode, Is.EqualTo(AspectRatioFitter.AspectMode.FitInParent));
            Assert.That(aspect.aspectRatio, Is.EqualTo(4f / 3f).Within(0.001f));
            Assert.That(baseLayer.parent, Is.SameAs(surface));
            Assert.That(restrictedLayer.parent, Is.SameAs(surface));
            Assert.That(technicalLayer.parent, Is.SameAs(surface));
            Assert.That(nodeRoot.parent, Is.SameAs(surface));

            var serialized = new SerializedObject(screen);
            AssertSerializedReference(serialized, "mapSurface", surface);
            AssertSerializedReference(serialized, "baseLayer", baseLayer.GetComponent<Image>());
            AssertSerializedReference(
                serialized,
                "restrictedLayer",
                restrictedLayer.GetComponent<Image>());
            AssertSerializedReference(
                serialized,
                "technicalLayer",
                technicalLayer.GetComponent<Image>());
            AssertSerializedReference(serialized, "nodeRoot", nodeRoot);
            AssertSerializedReference(serialized, "nodeTemplate", nodeTemplate.GetComponent<Button>());
            AssertSerializedReference(
                serialized,
                "selectionNameLabel",
                details.Find("Selection Name").GetComponent<Text>());
            AssertSerializedReference(
                serialized,
                "selectionStatusLabel",
                details.Find("Selection Status").GetComponent<Text>());
            AssertSerializedReference(
                serialized,
                "selectionDescriptionLabel",
                details.Find("Selection Description").GetComponent<Text>());
            AssertSerializedReference(
                serialized,
                "feedbackLabel",
                details.Find("Travel Feedback").GetComponent<Text>());
            AssertSerializedReference(
                serialized,
                "travelButton",
                details.Find("Confirm Travel Button").GetComponent<Button>());

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            Assert.That(buttons, Has.Some.Matches<Button>(button => button.name == "Deck07Button"));
            Assert.That(buttons, Has.Some.Matches<Button>(button => button.name == "BackButton"));

            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            Text currentLocation = root.transform.Find("Current Location").GetComponent<Text>();
            Text deckLabel = viewport.Find("Deck Label").GetComponent<Text>();
            Assert.That(viewportRect.anchorMax.y - viewportRect.anchorMin.y, Is.GreaterThanOrEqualTo(0.65f));
            Assert.That(currentLocation.fontSize, Is.GreaterThanOrEqualTo(28));
            Assert.That(deckLabel.fontSize, Is.GreaterThanOrEqualTo(24));
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [Test]
    public void AuthoredMapsOwnLocationNodesWithValidNormalizedPositions()
    {
        string[] paths = AssetDatabase.FindAssets("t:MapDefinition", new[]
            { "Assets/_Project/Content/Locations/Map" });
        Assert.That(paths, Has.Length.EqualTo(5));
        int nodes = 0;
        foreach (string guid in paths)
        {
            MapDefinition map = AssetDatabase.LoadAssetAtPath<MapDefinition>(
                AssetDatabase.GUIDToAssetPath(guid));
            Assert.That(map.Locations, Is.Not.Null, map.name);
            foreach (LocationDefinition location in map.Locations)
            {
                Assert.That(location, Is.Not.Null, map.name);
                Assert.That(location.MapNode, Is.Not.Null, location.name);
                Vector2 position = location.MapNode.NormalizedPosition;
                Assert.That(position.x, Is.InRange(0f, 1f), location.name);
                Assert.That(position.y, Is.InRange(0f, 1f), location.name);
                nodes++;
            }
        }
        Assert.That(nodes, Is.GreaterThanOrEqualTo(15));
    }

    [Test]
    public void AuthoredMapsUsePlayerFacingNamesAndCompleteBaseLayers()
    {
        MapDefinition[] maps = AssetDatabase.FindAssets(
                "t:MapDefinition",
                new[] { "Assets/_Project/Content/Locations/Map" })
            .Select(guid => AssetDatabase.LoadAssetAtPath<MapDefinition>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();

        Assert.That(maps, Has.Length.EqualTo(5));
        foreach (MapDefinition map in maps)
        {
            string authoredName = new SerializedObject(map)
                .FindProperty("displayName")
                .stringValue;
            Assert.That(authoredName, Is.Not.Empty, map.name);
            Assert.That(authoredName, Does.Not.Contain("MAP_"), map.name);
            Assert.That(map.BaseLayer, Is.Not.Null, map.name);
        }
    }

    [Test]
    public void MVElysiumUsesPortArtworkWithoutUnavailableOverlays()
    {
        MapDefinition map = AssetDatabase.LoadAssetAtPath<MapDefinition>(
            "Assets/_Project/Content/Locations/Map/MAP_MVElysium.asset");

        Assert.That(map, Is.Not.Null);
        Assert.That(map.DisplayName, Is.EqualTo("M.V. 엘리시움"));
        Assert.That(
            AssetDatabase.GetAssetPath(map.BaseLayer),
            Is.EqualTo("Assets/_Project/Art/Maps/DeckLayers/MAP_Port_Base.png"));
        Assert.That(map.RestrictedLayer, Is.Null);
        Assert.That(map.TechnicalLayer, Is.Null);
    }

    [Test]
    public void PortMapUsesAuthoredPortAndRouteOnlyGangwayNodes()
    {
        MapDefinition map = AssetDatabase.LoadAssetAtPath<MapDefinition>(
            "Assets/_Project/Content/Locations/Map/MAP_MVElysium.asset");
        LocationDefinition port = map.Locations.Single(location => location.Id == "LOC_PORT");
        LocationDefinition gangway = map.Locations.Single(
            location => location.Id == "LOC_GANGWAY");

        Assert.That(port.MapNode.NormalizedPosition.x, Is.EqualTo(0.2f).Within(0.001f));
        Assert.That(port.MapNode.NormalizedPosition.y, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(port.MapNode.DisplayName, Is.EqualTo("항구"));
        Assert.That(port.MapNode.Description, Is.Not.Empty);
        Assert.That(port.MapNode.AccessMode, Is.EqualTo(MapNodeAccessMode.PersistentUnlock));

        Assert.That(gangway.MapNode.NormalizedPosition.x, Is.EqualTo(0.58f).Within(0.001f));
        Assert.That(gangway.MapNode.NormalizedPosition.y, Is.EqualTo(0.51f).Within(0.001f));
        Assert.That(gangway.MapNode.DisplayName, Is.EqualTo("승선 통로"));
        Assert.That(gangway.MapNode.Description, Is.Not.Empty);
        Assert.That(gangway.MapNode.AccessMode, Is.EqualTo(MapNodeAccessMode.RouteOnly));
        Assert.That(gangway.DisplayName, Does.Not.Contain("LOC_"));
    }

    [TestCase("MAP_Deck07", "7층 갑판")]
    [TestCase("MAP_Deck10", "10층 갑판")]
    [TestCase("MAP_MVElysium", "M.V. 엘리시움")]
    public void DeckLabelsDoNotExposeInternalMapIds(string id, string expected)
    {
        Assert.That(MapScreen.FormatDeckLabel(id), Is.EqualTo(expected));
    }

    private static void AssertSerializedReference(
        SerializedObject serialized,
        string propertyName,
        UnityEngine.Object expected)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        Assert.That(property, Is.Not.Null, propertyName);
        Assert.That(property.objectReferenceValue, Is.SameAs(expected), propertyName);
    }
}

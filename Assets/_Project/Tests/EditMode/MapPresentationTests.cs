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
            Assert.That(root.GetComponent<MapScreen>(), Is.Not.Null);
            Assert.That(root.transform.Find("Map Viewport/Base Layer"), Is.Not.Null);
            Assert.That(root.transform.Find("Map Viewport/Restricted Layer"), Is.Not.Null);
            Assert.That(root.transform.Find("Map Viewport/Technical Layer"), Is.Not.Null);
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            Assert.That(buttons, Has.Some.Matches<Button>(button => button.name == "Deck07Button"));
            Assert.That(buttons, Has.Some.Matches<Button>(button => button.name == "BackButton"));

            RectTransform viewport = root.transform.Find("Map Viewport").GetComponent<RectTransform>();
            Text currentLocation = root.transform.Find("Current Location").GetComponent<Text>();
            Text deckLabel = root.transform.Find("Map Viewport/Deck Label").GetComponent<Text>();
            Assert.That(viewport.anchorMax.x - viewport.anchorMin.x, Is.GreaterThanOrEqualTo(0.78f));
            Assert.That(viewport.anchorMax.y - viewport.anchorMin.y, Is.GreaterThanOrEqualTo(0.73f));
            Assert.That(currentLocation.fontSize, Is.GreaterThanOrEqualTo(28));
            Assert.That(deckLabel.fontSize, Is.GreaterThanOrEqualTo(30));
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

    [TestCase("MAP_Deck07", "7층 갑판")]
    [TestCase("MAP_Deck10", "10층 갑판")]
    [TestCase("MAP_MVElysium", "M.V. 엘리시움")]
    public void DeckLabelsDoNotExposeInternalMapIds(string id, string expected)
    {
        Assert.That(MapScreen.FormatDeckLabel(id), Is.EqualTo(expected));
    }
}

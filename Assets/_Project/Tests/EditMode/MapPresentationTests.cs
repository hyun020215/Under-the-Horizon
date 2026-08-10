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
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}

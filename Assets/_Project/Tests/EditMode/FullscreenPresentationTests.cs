using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class FullscreenPresentationTests
{
    private const string GameScenePath =
        "Assets/_Project/Scenes/Game.unity";
    private const string TitlePrefabPath =
        "Assets/_Project/Prefabs/UI/PF_TitleScreen.prefab";

    [Test]
    public void GameShellFillsViewportWithoutFixedAspectFrames()
    {
        Scene scene = EditorSceneManager.OpenScene(
            GameScenePath,
            OpenSceneMode.Additive);

        try
        {
            Transform worldFrame = Find(scene, "WorldFrame");
            Transform uiFrame = Find(scene, "UIFrame");
            Transform background = Find(scene, "BackgroundLayer");
            Transform backgroundCharacters = Find(
                scene,
                "BackgroundCharacterFrame");
            Camera camera = Find(scene, "Main Camera").GetComponent<Camera>();

            AssertFullStretch(worldFrame);
            AssertFullStretch(uiFrame);
            Assert.That(
                background.GetComponent<AspectRatioFitter>().aspectMode,
                Is.EqualTo(AspectRatioFitter.AspectMode.EnvelopeParent));
            AspectRatioFitter backgroundCharacterAspect =
                backgroundCharacters.GetComponent<AspectRatioFitter>();
            Assert.That(
                backgroundCharacterAspect.aspectMode,
                Is.EqualTo(AspectRatioFitter.AspectMode.EnvelopeParent));
            Assert.That(
                backgroundCharacterAspect.aspectRatio,
                Is.EqualTo(background.GetComponent<AspectRatioFitter>().aspectRatio)
                    .Within(0.0001f));
            Assert.That(backgroundCharacters.parent.name,
                Is.EqualTo("CharacterLayer"));
            Assert.That(camera.backgroundColor.maxColorComponent, Is.LessThan(0.02f));
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void TitleArtworkCoversViewportWithoutDistortion()
    {
        GameObject root = UnityEditor.PrefabUtility.LoadPrefabContents(
            TitlePrefabPath);

        try
        {
            AspectRatioFitter artwork = root
                .GetComponentsInChildren<AspectRatioFitter>(true)
                .Single();

            Assert.That(
                artwork.aspectMode,
                Is.EqualTo(AspectRatioFitter.AspectMode.EnvelopeParent));
        }
        finally
        {
            UnityEditor.PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Transform Find(Scene scene, string name) =>
        scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Single(transform => transform.name == name);

    private static void AssertFullStretch(Transform transform)
    {
        RectTransform rect = (RectTransform)transform;
        Assert.That(rect.anchorMin, Is.EqualTo(Vector2.zero));
        Assert.That(rect.anchorMax, Is.EqualTo(Vector2.one));
        Assert.That(rect.sizeDelta, Is.EqualTo(Vector2.zero));
        Assert.That(rect.GetComponent<AspectRatioFitter>(), Is.Null);
    }
}

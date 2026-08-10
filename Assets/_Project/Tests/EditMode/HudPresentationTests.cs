using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class HudPresentationTests
{
    [Test]
    public void HudShowsContextAndObjectiveWithoutInternalNumericMeters()
    {
        Scene scene = EditorSceneManager.OpenScene(
            "Assets/_Project/Scenes/Game.unity", OpenSceneMode.Additive);
        try
        {
            Transform hud = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Single(item => item.name == "PersistentHUD");
            Assert.That(hud.Find("StatusBar/TimePanel"), Is.Not.Null);
            Assert.That(hud.Find("StatusBar/LocationPanel"), Is.Not.Null);
            Assert.That(hud.Find("StatusBar/ObjectivePanel"), Is.Not.Null);
            Assert.That(hud.Find("StatusBar/AnxietyPanel"), Is.Null);
            Assert.That(hud.Find("StatusBar/IntegrityPanel"), Is.Null);
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}

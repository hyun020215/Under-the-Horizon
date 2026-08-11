using System.Linq;
using NUnit.Framework;
using UnityEditor;
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

    [Test]
    public void ObjectiveGuidanceUsesSceneInteractionProgress()
    {
        StorySceneDefinition scene = AssetDatabase.LoadAssetAtPath<StorySceneDefinition>(
            "Assets/_Project/Content/StoryScenes/Day01/D1_06_BodyDiscovery.asset");
        Assert.That(scene, Is.Not.Null);
        Assert.That(scene.InteractionSet, Is.Not.Null);

        GameObject root = new("Objective State", typeof(GameStateStore));
        try
        {
            GameStateStore state = root.GetComponent<GameStateStore>();
            ObjectiveGuidance initial = ObjectiveGuidanceResolver.Resolve(scene, state);
            Assert.That(initial.Objective, Is.EqualTo(scene.DisplayName));
            Assert.That(initial.TotalSteps, Is.GreaterThan(0));
            Assert.That(initial.Guidance, Is.Not.Empty);

            InteractionDefinition first = scene.InteractionSet.Interactions
                .First(interaction => interaction != null && interaction.IsAvailable(state));
            state.CompleteInteraction(first.Id);
            ObjectiveGuidance advanced = ObjectiveGuidanceResolver.Resolve(scene, state);
            Assert.That(advanced.CompletedSteps, Is.EqualTo(initial.CompletedSteps + 1));
            Assert.That(advanced.HudText, Does.Contain("/"));
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }
}

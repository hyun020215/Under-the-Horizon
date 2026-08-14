using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

            RectTransform statusBar = hud.Find("StatusBar").GetComponent<RectTransform>();
            Image statusBackground = statusBar.GetComponent<Image>();
            Outline goldRule = statusBar.GetComponent<Outline>();
            Assert.That(statusBar.anchorMin.y, Is.GreaterThanOrEqualTo(0.88f));
            Assert.That(statusBackground.color.a, Is.LessThanOrEqualTo(0.15f));
            Assert.That(goldRule, Is.Not.Null);
            Assert.That(goldRule.effectColor.r, Is.GreaterThan(goldRule.effectColor.b));
            foreach (string panelName in new[] { "TimePanel", "LocationPanel", "ObjectivePanel" })
            {
                Image panel = hud.Find($"StatusBar/{panelName}").GetComponent<Image>();
                Assert.That(panel.color.a, Is.LessThanOrEqualTo(0.16f));
            }
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

    [Test]
    public void PendingTravelGuidanceUsesThePlayerFacingDestinationName()
    {
        LocationDefinition location = ScriptableObject.CreateInstance<LocationDefinition>();
        StorySceneDefinition target = ScriptableObject.CreateInstance<StorySceneDefinition>();
        try
        {
            SetPrivateField(location, "id", "LOC_GANGWAY");
            SetPrivateField(location, "displayName", "승선 통로");
            SetPrivateField(target, "id", "P-02");
            SetPrivateField(target, "location", location);

            ObjectiveGuidance guidance = ObjectiveGuidanceResolver.Resolve(
                new PendingStorySceneTravel(null, target));

            Assert.That(guidance.Objective, Is.EqualTo("승선 통로로 향하기"));
            Assert.That(guidance.Guidance, Is.EqualTo("지도에서 목적지를 선택해 이동하기"));
            Assert.That(guidance.HudText, Does.Not.Contain("LOC_"));
        }
        finally
        {
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(location);
        }
    }

    [Test]
    public void HudDoesNotExposeAnUnknownLocationId()
    {
        var root = new GameObject("HUD fallback test");
        root.SetActive(false);
        ContentDatabase content = ScriptableObject.CreateInstance<ContentDatabase>();
        try
        {
            GameStateStore state = root.AddComponent<GameStateStore>();
            PersistentHud hud = root.AddComponent<PersistentHud>();
            var labelOwner = new GameObject("Location", typeof(RectTransform), typeof(Text));
            labelOwner.transform.SetParent(root.transform, false);
            Text locationLabel = labelOwner.GetComponent<Text>();
            SetPrivateField(hud, "state", state);
            SetPrivateField(hud, "content", content);
            SetPrivateField(hud, "locationLabel", locationLabel);
            state.SetCurrentLocation("LOC_INTERNAL_ONLY");

            InvokePrivate(hud, "Refresh", state.State);

            Assert.That(locationLabel.text, Is.EqualTo("알 수 없는 위치"));
            Assert.That(locationLabel.text, Does.Not.Contain("LOC_"));
        }
        finally
        {
            Object.DestroyImmediate(content);
            Object.DestroyImmediate(root);
        }
    }

    private static void SetPrivateField<T>(object target, string name, T value)
    {
        FieldInfo field = target.GetType().GetField(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, name);
        field.SetValue(target, value);
    }

    private static void InvokePrivate(object target, string name, params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, name);
        method.Invoke(target, arguments);
    }
}

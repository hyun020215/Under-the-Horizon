using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class ScreenTransitionRoutingTests
{
    [Test]
    public void StandardFadeAuthorsAllFivePresentationPhases()
    {
        TransitionProfile profile = AssetDatabase.LoadAssetAtPath<TransitionProfile>(
            "Assets/_Project/Content/Transitions/TRANS_FADE_STANDARD.asset");
        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.uiExitDuration, Is.GreaterThan(0f));
        Assert.That(profile.coverDuration, Is.GreaterThan(profile.uiExitDuration));
        Assert.That(profile.holdDuration, Is.GreaterThan(0f));
        Assert.That(profile.revealDuration, Is.GreaterThan(0f));
        Assert.That(profile.uiEnterDuration, Is.GreaterThan(0f));
        Assert.That(profile.coverColor.b, Is.GreaterThan(profile.coverColor.r));
        Assert.That(profile.particleCount, Is.GreaterThanOrEqualTo(10));

        string director = File.ReadAllText(
            "Assets/_Project/Runtime/Transitions/TransitionDirector.cs");
        Assert.That(director, Does.Contain("WaitAsync(profile.uiExitDuration)"));
        Assert.That(director, Does.Contain("WaitAsync(profile.holdDuration)"));
        Assert.That(director, Does.Contain("WaitAsync(profile.uiEnterDuration)"));
        string fade = File.ReadAllText(
            "Assets/_Project/Runtime/Transitions/FadeTransitionPlayer.cs");
        Assert.That(fade, Does.Contain("Transition Particle"));
        Assert.That(fade, Does.Contain("request.ReducedMotion"));
        Assert.That(fade, Does.Contain("UiGlowSprite.Get()"));
        string ambient = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Components/AmbientParticleOverlay.cs");
        Assert.That(ambient, Does.Contain("UiGlowSprite.Get()"));
    }

    [Test]
    public void GameRouterUsesAuthoredProfilesForFeatureScreens()
    {
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/Game.unity", OpenSceneMode.Single);
        ScreenRouter router = Object.FindFirstObjectByType<ScreenRouter>();
        Assert.That(router, Is.Not.Null);
        SerializedProperty routes = new SerializedObject(router).FindProperty("transitionRoutes");
        var mapped = new Dictionary<ScreenId, string>();
        for (var index = 0; index < routes.arraySize; index++)
        {
            SerializedProperty route = routes.GetArrayElementAtIndex(index);
            var id = (ScreenId)route.FindPropertyRelative("screen").enumValueIndex;
            var profile = (TransitionProfile)route.FindPropertyRelative("profile").objectReferenceValue;
            mapped[id] = profile?.name;
        }
        Assert.That(mapped[ScreenId.Investigation], Is.EqualTo("TRANS_INVESTIGATION_OPEN"));
        Assert.That(mapped[ScreenId.InvestigationRecord], Is.EqualTo("TRANS_DISCOVERY"));
        Assert.That(mapped[ScreenId.EvidenceBoard], Is.EqualTo("TRANS_DISCOVERY"));
        Assert.That(mapped[ScreenId.Puzzle], Is.EqualTo("TRANS_PUZZLE_OPEN"));
        Assert.That(mapped[ScreenId.Ending], Is.EqualTo("TRANS_ENDING"));
    }
}

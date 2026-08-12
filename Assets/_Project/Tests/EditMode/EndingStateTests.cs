using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class EndingStateTests
{
    private GameObject host;
    private GameStateStore state;

    [SetUp]
    public void SetUp()
    {
        host = new GameObject(nameof(EndingStateTests));
        state = host.AddComponent<GameStateStore>();
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(host);

    [Test]
    public void TrySetEnding_IsWriteOnce()
    {
        Assert.That(state.TrySetEnding(" ending_a_complete "), Is.True);
        Assert.That(state.TrySetEnding("ending_c_wrong_person"), Is.False);
        Assert.That(state.State.endingId, Is.EqualTo("ending_a_complete"));
    }

    [Test]
    public void TrySetEnding_RejectsBlankId()
    {
        Assert.That(state.TrySetEnding("  "), Is.False);
        Assert.That(state.State.endingId, Is.Empty);
    }

    [TestCase("ending_a_complete", "D8-02")]
    [TestCase("ending_b_convenient_culprit", "D8-02")]
    [TestCase("ending_c_wrong_person", "D8-03")]
    [TestCase("ending_bad_panic", "D8-03")]
    public void FinalInterrogationRoutesEachOfficialEnding(string endingId, string expectedScene)
    {
        StorySceneDefinition scene = LoadFinalInterrogation();
        state.TrySetEnding(endingId);
        Assert.That(scene.ResolveNext(state), Is.EqualTo(expectedScene));
    }

    [Test]
    public void FinalInterrogationResolvesCompleteEndingFromAuthoredChoicesAndTheories()
    {
        StorySceneDefinition scene = LoadFinalInterrogation();
        foreach (string choice in CorrectChoices)
            state.SetFlag($"DIALOGUE_CHOICE_{choice}");
        foreach (string theory in CrimeTheories.Append("past_event"))
            state.ResolveTheory(theory);
        state.SetFlag("DIALOGUE_CHOICE_D8-01_END_A");

        ApplyCompletion(scene);

        Assert.That(state.State.endingId, Is.EqualTo("ending_a_complete"));
        Assert.That(scene.ResolveNext(state), Is.EqualTo("D8-02"));
    }

    [Test]
    public void FinalInterrogationPanicOverridesCorrectArgument()
    {
        StorySceneDefinition scene = LoadFinalInterrogation();
        foreach (string choice in CorrectChoices)
            state.SetFlag($"DIALOGUE_CHOICE_{choice}");
        foreach (string theory in CrimeTheories.Append("past_event"))
            state.ResolveTheory(theory);
        state.SetFlag("DIALOGUE_CHOICE_D8-01_END_A");
        state.ChangeAnxiety(100);

        ApplyCompletion(scene);

        Assert.That(state.State.endingId, Is.EqualTo("ending_bad_panic"));
        Assert.That(scene.ResolveNext(state), Is.EqualTo("D8-03"));
    }

    private static readonly string[] CorrectChoices =
    {
        "D8-01_A1_EVELYN", "D8-01_A2_BALLAST", "D8-01_A3_SUFFOCATION",
        "D8-01_A4_RAIL", "D8-01_A5_MISCONCEPTION", "D8-01_A6_EVELYN"
    };

    private static readonly string[] CrimeTheories =
    {
        "scene_denial", "body_insertion", "transport_route", "actual_murder", "culprit_link"
    };

    private static StorySceneDefinition LoadFinalInterrogation() =>
        AssetDatabase.LoadAssetAtPath<StorySceneDefinition>(
            "Assets/_Project/Content/StoryScenes/Day08/D8_01_FinalInterrogation.asset");

    private void ApplyCompletion(StorySceneDefinition scene)
    {
        foreach (GameEffect effect in scene.OnCompleteEffects)
            effect.Apply(state);
    }
}

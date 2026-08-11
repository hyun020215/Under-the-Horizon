using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class EvidenceDiscoveryPresentationTests
{
    [Test]
    public void GameSceneProvidesOneEvidenceDiscoveryOverlay()
    {
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/Game.unity", OpenSceneMode.Single);
        Assert.That(Object.FindObjectsByType<EvidenceDiscoveryPresenter>(
            FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(1));
        GameObject overlay = GameObject.Find("EvidenceDiscoveryOverlay");
        Assert.That(overlay?.transform.Find("Card/Evidence Image"), Is.Not.Null);
        Assert.That(overlay?.transform.Find("Card/Title"), Is.Not.Null);
        Assert.That(overlay?.transform.Find("Card/Description"), Is.Not.Null);
    }

    [Test]
    public void EvidenceNotificationOriginatesFromAuthoritativeStateMutation()
    {
        string state = File.ReadAllText("Assets/_Project/Runtime/State/GameStateStore.cs");
        string director = File.ReadAllText("Assets/_Project/Runtime/Evidence/EvidenceDirector.cs");
        string presenter = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Components/EvidenceDiscoveryPresenter.cs");
        Assert.That(state, Does.Contain("EvidenceAdded?.Invoke(normalized)"));
        Assert.That(director, Does.Contain("state.EvidenceAdded += OnEvidenceAdded"));
        Assert.That(presenter, Does.Contain("evidence.EvidenceDiscovered += Present"));
        Assert.That(presenter, Does.Not.Contain("AddEvidence"));
    }

    [Test]
    public void TheoryReadyNotificationReusesEvidenceBoardEvaluationAndDiscoveryOverlay()
    {
        string board = File.ReadAllText(
            "Assets/_Project/Runtime/Evidence/EvidenceBoardDirector.cs");
        string presenter = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Components/EvidenceDiscoveryPresenter.cs");
        Assert.That(board, Does.Contain("foreach (TheoryEvaluation evaluation in EvaluateTheories())"));
        Assert.That(board, Does.Contain("TheoryReady?.Invoke(theory)"));
        Assert.That(presenter, Does.Contain("board.TheoryReady += PresentTheory"));
        Assert.That(presenter, Does.Contain("DEDUCTION READY"));
        Assert.That(presenter, Does.Not.Contain("SetFlag"));
    }
}

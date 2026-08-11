using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class InteractionFeedbackArchitectureTests
{
    [Test]
    public void GameSceneOwnsOneSharedInteractionFeedbackService()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
            "Assets/_Project/Scenes/Game.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
        Assert.That(Object.FindObjectsByType<InteractionFeedbackService>(
            FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(1));
        Assert.That(scene.IsValid(), Is.True);
    }

    [Test]
    public void InteractionViewDelegatesPresentationAndKeepsExecutionInDirector()
    {
        string view = File.ReadAllText(
            "Assets/_Project/Runtime/Interaction/InteractionPointView.cs");
        Assert.That(view, Does.Contain("InteractionFeedbackService"));
        Assert.That(view, Does.Contain("Clicked?.Invoke(this)"));
        Assert.That(view, Does.Not.Contain("ExecuteAsync"));
        Assert.That(view, Does.Not.Contain("CompleteInteraction"));
    }
}

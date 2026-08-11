using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public sealed class EvidenceBoardPresentationTests
{
    [Test]
    public void BoardPrefabProvidesEvidenceNodesTheorySlotsAndConnectionDetail()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(
            "Assets/_Project/Prefabs/UI/PF_EvidenceBoardScreen.prefab");
        try
        {
            Assert.That(root.GetComponentsInChildren<Button>(true)
                .Count(item => item.name.StartsWith("EvidenceNode")), Is.EqualTo(18));
            Assert.That(root.GetComponentsInChildren<Button>(true)
                .Count(item => item.name.StartsWith("TheorySlot")), Is.EqualTo(6));
            Assert.That(root.transform.Find("Connection Detail/Detail Title"), Is.Not.Null);
            Assert.That(root.transform.Find("Connection Detail/Detail Body"), Is.Not.Null);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [Test]
    public void BoardViewUsesDirectorAndRouterWithoutMutatingGameState()
    {
        string source = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Screens/EvidenceBoardScreen.cs");
        Assert.That(source, Does.Contain("board?.Discovered"));
        Assert.That(source, Does.Contain("screens.OpenAsync"));
        Assert.That(source, Does.Not.Contain("GameStateStore"));
        Assert.That(source, Does.Not.Contain("SetFlag"));
        Assert.That(source, Does.Not.Contain("AddEvidence"));
        Assert.That(source, Does.Contain("board.TryResolve"));
        Assert.That(source, Does.Not.Contain("ResolveTheory("));
    }
}

using System.Linq;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public sealed class InvestigationRecordPresentationTests
{
    [Test]
    public void RecordPrefabProvidesHiddenEvidenceCapacityAndDetailView()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(
            "Assets/_Project/Prefabs/UI/PF_RecordScreen.prefab");
        try
        {
            Button[] cards = root.GetComponentsInChildren<Button>(true)
                .Where(button => button.name.StartsWith("EvidenceCard"))
                .ToArray();
            Assert.That(cards.Length, Is.EqualTo(18));
            Assert.That(root.transform.Find("Evidence Detail/Detail Image"), Is.Not.Null);
            Assert.That(root.transform.Find("Evidence Detail/Detail Title"), Is.Not.Null);
            Assert.That(root.transform.Find("Evidence Detail/Detail Body"), Is.Not.Null);
            Assert.That(root.transform.Find("Evidence List/Empty Label"), Is.Not.Null);
            Assert.That(root.transform.Find("EvidenceFilterButton"), Is.Not.Null);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [Test]
    public void EveryEvidenceHasCanonicalClassificationMetadata()
    {
        EvidenceDefinition[] evidence = AssetDatabase.FindAssets("t:EvidenceDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<EvidenceDefinition>)
            .Where(item => item != null)
            .ToArray();
        Assert.That(evidence, Has.Length.EqualTo(18));
        Assert.That(evidence, Has.All.Matches<EvidenceDefinition>(
            item => !string.IsNullOrWhiteSpace(item.Category)));
        Assert.That(evidence.Count(item => item.IsDirect), Is.EqualTo(9));
    }

    [Test]
    public void RecordMarksUnviewedEvidenceWithoutAddingSaveState()
    {
        string source = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Screens/InvestigationRecordScreen.cs");
        Assert.That(source, Does.Contain("HashSet<string> viewedEvidence"));
        Assert.That(source, Does.Contain("NEW ·"));
        Assert.That(source, Does.Contain("accessibility?.ReducedMotion"));

        string gameState = File.ReadAllText(
            "Assets/_Project/Runtime/State/GameState.cs");
        Assert.That(gameState, Does.Not.Contain("viewedEvidence"));
    }
}

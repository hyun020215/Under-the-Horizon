using System.Linq;
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
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}

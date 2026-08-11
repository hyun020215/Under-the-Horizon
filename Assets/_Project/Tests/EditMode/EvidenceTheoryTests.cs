using System.Linq;
using NUnit.Framework;
using UnityEditor;

public sealed class EvidenceTheoryTests
{
    [Test]
    public void CoreEvidenceIdsRemainCanonicalAndComplete()
    {
        string[] actual = AssetDatabase
            .FindAssets("t:EvidenceDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<EvidenceDefinition>)
            .Where(item => item != null)
            .Select(item => item.Id)
            .OrderBy(id => id)
            .ToArray();
        string[] expected = Enumerable.Range(1, 18)
            .Select(number => $"C-{number:00}")
            .ToArray();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BoardGraphRequiresTheExactAuthoredEvidenceSet()
    {
        TheoryDefinition theory = AssetDatabase
            .FindAssets("t:TheoryDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<TheoryDefinition>)
            .First(item => item.RequiredEvidence.Length > 1);
        var graph = new EvidenceBoardGraph();
        foreach (EvidenceDefinition evidence in theory.RequiredEvidence)
            graph.Toggle(evidence);
        Assert.That(graph.Matches(theory), Is.True);
        graph.Toggle(theory.RequiredEvidence[0]);
        Assert.That(graph.Matches(theory), Is.False);
    }

    [Test]
    public void EveryTheoryResolvesThroughAContentEffect()
    {
        TheoryDefinition[] theories = AssetDatabase
            .FindAssets("t:TheoryDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<TheoryDefinition>)
            .Where(item => item != null)
            .ToArray();
        foreach (TheoryDefinition theory in theories)
        {
            ResolveTheoryEffect effect = theory.OnResolvedEffects
                .OfType<ResolveTheoryEffect>()
                .SingleOrDefault();
            Assert.That(effect, Is.Not.Null, theory.name);
            Assert.That(effect.TheoryId, Is.EqualTo(theory.Id), theory.name);
        }
    }

    [Test]
    public void CanonicalTheoriesHaveUniqueIdsAndEvidenceRequirements()
    {
        TheoryDefinition[] theories = AssetDatabase
            .FindAssets("t:TheoryDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<TheoryDefinition>)
            .Where(item => item != null)
            .ToArray();

        Assert.That(theories, Has.Length.EqualTo(6));
        Assert.That(theories.Select(item => item.Id), Is.Unique);
        Assert.That(theories, Has.All.Matches<TheoryDefinition>(
            item => !string.IsNullOrWhiteSpace(item.Id) &&
                    !string.IsNullOrWhiteSpace(item.DisplayName) &&
                    item.RequiredEvidence.Length > 0));
        Assert.That(theories.SelectMany(item => item.RequiredEvidence),
            Has.All.Not.Null);
    }
}

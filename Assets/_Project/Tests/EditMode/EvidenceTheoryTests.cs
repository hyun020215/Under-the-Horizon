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
}

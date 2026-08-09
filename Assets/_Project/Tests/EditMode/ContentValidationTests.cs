using System.Linq;
using NUnit.Framework;
using UnityEditor;

public sealed class ContentValidationTests
{
    [Test]
    public void AllAuthoredContentPassesBuildPreflightRules()
    {
        var errors = ContentValidator.ValidateAll();
        Assert.That(errors, Is.Empty, string.Join("\n", errors));
    }

    [Test]
    public void OfficialDialogueChoicesAreImportedAsExecutableGraphNodes()
    {
        DialogueChoice[] choices = AssetDatabase
            .FindAssets("t:DialogueSequence")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<DialogueSequence>)
            .Where(sequence => sequence?.Lines != null)
            .SelectMany(sequence => sequence.Lines)
            .Where(line => line.choices != null)
            .SelectMany(line => line.choices)
            .Where(choice => choice != null)
            .ToArray();

        Assert.That(choices, Has.Length.EqualTo(100));
        Assert.That(choices.Select(choice => choice.Id), Is.Unique);
        Assert.That(
            choices.All(choice => !string.IsNullOrWhiteSpace(choice.Text)),
            Is.True);
    }
}

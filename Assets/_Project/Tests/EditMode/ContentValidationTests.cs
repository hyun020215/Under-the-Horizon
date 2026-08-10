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

    [Test]
    public void EveryAssignedPuzzleHasAReachableWorldInteraction()
    {
        StorySceneDefinition[] puzzleScenes = AssetDatabase
            .FindAssets("t:StorySceneDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<StorySceneDefinition>)
            .Where(scene => scene?.Puzzle != null)
            .ToArray();

        Assert.That(puzzleScenes, Has.Length.EqualTo(13));
        foreach (StorySceneDefinition scene in puzzleScenes)
        {
            InteractionDefinition interaction = scene.InteractionSet.Interactions
                .SingleOrDefault(item =>
                    item?.Action is PuzzleInteractionAction action
                    && action.Puzzle == scene.Puzzle);

            Assert.That(interaction, Is.Not.Null, scene.Id);
            Assert.That(interaction.HasWorldHotspot, Is.True, scene.Id);
        }
    }
}

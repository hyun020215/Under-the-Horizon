using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

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

    [Test]
    public void PlacementValidationRejectsUnknownCoordinateSpace()
    {
        StorySceneDefinition scene =
            ScriptableObject.CreateInstance<StorySceneDefinition>();
        CharacterPlacementSet set =
            ScriptableObject.CreateInstance<CharacterPlacementSet>();

        try
        {
            SetPrivateField(
                set,
                "placementSpace",
                (CharacterPlacementSpace)99);
            SetPrivateField(scene, "characterSet", set);

            List<string> errors = InvokePlacementValidation(scene);

            Assert.That(
                errors.Any(error =>
                    error.Contains("unsupported character placement space")),
                Is.True);
        }
        finally
        {
            Object.DestroyImmediate(scene);
            Object.DestroyImmediate(set);
        }
    }

    [Test]
    public void PlacementValidationRequiresEffectiveBackgroundForOptIn()
    {
        StorySceneDefinition scene =
            ScriptableObject.CreateInstance<StorySceneDefinition>();
        CharacterPlacementSet set =
            ScriptableObject.CreateInstance<CharacterPlacementSet>();

        try
        {
            SetPrivateField(
                set,
                "placementSpace",
                CharacterPlacementSpace.BackgroundNormalized);
            SetPrivateField(scene, "characterSet", set);

            List<string> errors = InvokePlacementValidation(scene);

            Assert.That(
                errors.Any(error =>
                    error.Contains("without an effective background")),
                Is.True);
        }
        finally
        {
            Object.DestroyImmediate(scene);
            Object.DestroyImmediate(set);
        }
    }

    private static List<string> InvokePlacementValidation(
        StorySceneDefinition scene)
    {
        MethodInfo validate = typeof(ContentValidator).GetMethod(
            "ValidatePlacements",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(validate, Is.Not.Null);
        var errors = new List<string>();
        validate.Invoke(null, new object[] { scene, errors });
        return errors;
    }

    private static void SetPrivateField(
        object target,
        string name,
        object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
        ?.SetValue(target, value);
}

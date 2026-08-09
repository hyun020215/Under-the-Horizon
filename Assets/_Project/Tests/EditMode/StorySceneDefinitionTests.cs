using System.Linq;
using NUnit.Framework;
using UnityEditor;

public sealed class StorySceneDefinitionTests
{
    [Test]
    public void CanonicalStoryScenesAreDataAssetsWithRequiredLinks()
    {
        StorySceneDefinition[] scenes = AssetDatabase
            .FindAssets("t:StorySceneDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<StorySceneDefinition>)
            .Where(scene => scene != null)
            .ToArray();

        Assert.That(scenes, Has.Length.EqualTo(41));
        Assert.That(scenes.Select(scene => scene.Id), Is.Unique);
        Assert.That(scenes.All(scene => scene.Location != null), Is.True);
        Assert.That(scenes.All(scene => scene.LocationState != null), Is.True);
        Assert.That(scenes.All(scene => scene.EntryDialogue != null), Is.True);
        Assert.That(scenes.All(scene => scene.InteractionSet != null), Is.True);
    }

    [Test]
    public void DeclaredAuthoringRequirementsAreSatisfied()
    {
        StorySceneDefinition[] scenes = AssetDatabase
            .FindAssets("t:StorySceneDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<StorySceneDefinition>)
            .Where(scene => scene?.AuthoringRequirements != null)
            .ToArray();

        Assert.That(scenes, Has.Length.EqualTo(41));
        foreach (StorySceneDefinition scene in scenes)
        {
            StorySceneAuthoringRequirements requirements =
                scene.AuthoringRequirements;
            int interactions = scene.InteractionSet?.Interactions?.Length ?? 0;
            Assert.That(
                interactions,
                Is.GreaterThanOrEqualTo(requirements.MinimumInteractionCount),
                scene.Id);

            if (requirements.RequiresPuzzle)
                Assert.That(scene.Puzzle, Is.Not.Null, scene.Id);
            if (requirements.RequiresEntrySequence)
                Assert.That(scene.EntrySequence, Is.Not.Null, scene.Id);
            if (requirements.RequiresExitSequence)
                Assert.That(scene.ExitSequence, Is.Not.Null, scene.Id);
        }
    }

    [Test]
    public void D106UsesAuthoredInvestigationCapabilitiesAndCrimeSceneState()
    {
        StorySceneDefinition scene = AssetDatabase
            .FindAssets("D1_06_BodyDiscovery t:StorySceneDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<StorySceneDefinition>)
            .Single();

        Assert.That(scene.LocationState.Id, Is.EqualTo("HORIZON_CRIME_SCENE"));
        Assert.That(scene.DeferEntryDialogue, Is.True);
        Assert.That(scene.InteractionSet.Interactions, Has.Length.EqualTo(6));
        Assert.That(
            scene.InteractionSet.Interactions.Select(item => item.Type),
            Does.Contain(InteractionType.Character));
        Assert.That(
            scene.InteractionSet.Interactions.Select(item => item.Type),
            Does.Contain(InteractionType.Context));
        Assert.That(
            scene.InteractionSet.Interactions.Select(item => item.Type),
            Does.Contain(InteractionType.Investigation));
        Assert.That(
            scene.InteractionSet.Interactions.Any(item => item.Action.GrantsEvidence),
            Is.True);
        Assert.That(
            scene.InteractionSet.Interactions.Count(item => item.HasWorldHotspot),
            Is.EqualTo(5));
    }

    [Test]
    public void D106RecreatesTheFourFrameBodyDiscoveryMontage()
    {
        StorySceneDefinition scene = AssetDatabase
            .FindAssets("D1_06_BodyDiscovery t:StorySceneDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<StorySceneDefinition>)
            .Single();
        ImageMontageCommand montage = scene.EntrySequence.Commands
            .OfType<ImageMontageCommand>()
            .Single();

        Assert.That(montage.Frames, Has.Length.EqualTo(4));
        Assert.That(montage.Frames, Has.All.Not.Null);
        Assert.That(
            montage.Frames.Select(frame => frame.name),
            Is.EqualTo(new[]
            {
                "EVD_discovery1",
                "EVD_discovery2",
                "EVD_discovery3",
                "EVD_discovery4"
            }));
        Assert.That(
            montage.HoldSeconds,
            Is.EqualTo(new[] { 1.2f, 1.35f, 1.3f, 1.75f }));
        Assert.That(
            montage.SeenFlag,
            Is.EqualTo("cinematic.d1_06_body_discovery_seen"));
    }
}

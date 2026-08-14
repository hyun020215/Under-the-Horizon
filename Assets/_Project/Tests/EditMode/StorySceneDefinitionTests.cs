using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

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

    [Test]
    public void P01UsesFourAuthoredStepsAndP02RequiresItsCompletion()
    {
        StorySceneDefinition p01 = AssetDatabase.LoadAssetAtPath<StorySceneDefinition>(
            "Assets/_Project/Content/StoryScenes/Prologue/P01_PortJournalist.asset");
        StorySceneDefinition p02 = AssetDatabase.LoadAssetAtPath<StorySceneDefinition>(
            "Assets/_Project/Content/StoryScenes/Prologue/P02_GangwayManifest.asset");
        var owner = new GameObject("P01ContentProgressionTest");

        try
        {
            GameStateStore state = owner.AddComponent<GameStateStore>();
            InteractionDefinition[] interactions = p01.InteractionSet.Interactions;

            Assert.That(p01.DeferEntryDialogue, Is.True);
            Assert.That(p01.EntrySequence, Is.Not.Null);
            DialogueCommand opening = p01.EntrySequence.Commands
                .OfType<DialogueCommand>()
                .Single();
            AssertPrivateString(opening, "startLineId", "P-01_001");
            AssertPrivateString(opening, "endLineId", "P-01_002");

            Assert.That(
                interactions.Select(interaction => interaction.Id),
                Is.EqualTo(new[]
                {
                    "INT_P_01_INVITATION",
                    "INT_P_01_MESSENGER",
                    "INT_P_01_DIALOGUE",
                    "INT_P_01_CONTINUE",
                }));
            Assert.That(
                interactions.Select(interaction => interaction.Type),
                Is.EqualTo(new[]
                {
                    InteractionType.Investigation,
                    InteractionType.Context,
                    InteractionType.Character,
                    InteractionType.Exit,
                }));
            Assert.That(interactions[0].Action.GrantsEvidence, Is.True);
            Assert.That(interactions[0].HasWorldHotspot, Is.True);
            Assert.That(interactions[0].TargetId, Is.EqualTo("C-01"));
            Assert.That(
                interactions[0].NormalizedRect,
                Is.EqualTo(new Rect(0.012f, 0.182f, 0.066f, 0.086f)),
                "The invitation hotspot must match the manually approved C-01 semantic region.");
            Assert.That(
                interactions[3].Action,
                Is.TypeOf<StorySceneAdvanceInteractionAction>());
            Assert.That(interactions[3].HasWorldHotspot, Is.True);
            Assert.That(interactions[3].TargetId, Is.EqualTo("LOC_GANGWAY"));
            Assert.That(
                interactions[3].NormalizedRect,
                Is.EqualTo(new Rect(0.38f, 0.36f, 0.24f, 0.25f)),
                "The continue hotspot must match the manually approved gangway semantic region.");
            Assert.That(interactions.All(interaction => !interaction.Repeatable), Is.True);

            AssertActionRange(interactions[0].Action, "P-01_003", "P-01_005");
            AssertActionRange(interactions[1].Action, "P-01_006", "P-01_008");
            AssertActionRange(interactions[2].Action, "P-01_009", "P-01_026");

            Assert.That(interactions[0].IsAvailable(state), Is.True);
            Assert.That(interactions.Skip(1).All(item => !item.IsAvailable(state)), Is.True);
            state.CompleteInteraction(interactions[0].Id);
            Assert.That(interactions[1].IsAvailable(state), Is.True);
            Assert.That(interactions.Skip(2).All(item => !item.IsAvailable(state)), Is.True);
            state.CompleteInteraction(interactions[1].Id);
            Assert.That(interactions[2].IsAvailable(state), Is.True);
            Assert.That(interactions[3].IsAvailable(state), Is.False);
            state.CompleteInteraction(interactions[2].Id);
            Assert.That(interactions[3].IsAvailable(state), Is.True);

            Assert.That(p01.Routes.Single().TargetSceneId, Is.EqualTo("P-02"));
            Assert.That(ConditionResolver.All(p02.EntryConditions, state), Is.False);
            state.CompleteScene("P-01");
            Assert.That(ConditionResolver.All(p02.EntryConditions, state), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    private static void AssertActionRange(
        InteractionAction action,
        string expectedStart,
        string expectedEnd)
    {
        SerializedObject serialized = new(action);
        Assert.That(
            serialized.FindProperty("startLineId").stringValue,
            Is.EqualTo(expectedStart));
        Assert.That(
            serialized.FindProperty("endLineId").stringValue,
            Is.EqualTo(expectedEnd));
    }

    private static void AssertPrivateString(
        object target,
        string fieldName,
        string expected)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}'.");
        Assert.That(field.GetValue(target), Is.EqualTo(expected));
    }
}

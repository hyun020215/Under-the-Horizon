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
    public void P01UsesThreeAuthoredStepsAndDefersP02DialogueToDaniel()
    {
        StorySceneDefinition p01 = AssetDatabase.LoadAssetAtPath<StorySceneDefinition>(
            "Assets/_Project/Content/StoryScenes/Prologue/P01_PortJournalist.asset");
        StorySceneDefinition p02 = AssetDatabase.LoadAssetAtPath<StorySceneDefinition>(
            "Assets/_Project/Content/StoryScenes/Prologue/P02_GangwayManifest.asset");
        var owner = new GameObject("P01ContentProgressionTest");

        try
        {
            GameStateStore state = owner.AddComponent<GameStateStore>();
            InteractionDirector director = owner.AddComponent<InteractionDirector>();
            var directorData = new SerializedObject(director);
            directorData.FindProperty("state").objectReferenceValue = state;
            directorData.ApplyModifiedPropertiesWithoutUndo();
            director.Apply(p01.InteractionSet);
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
                }));
            Assert.That(
                interactions.Select(interaction => interaction.Type),
                Is.EqualTo(new[]
                {
                    InteractionType.Investigation,
                    InteractionType.Context,
                    InteractionType.Character,
                }));
            Assert.That(
                interactions.Select(interaction => interaction.DisplayName),
                Is.EqualTo(new[]
                {
                    "DANIEL MERCER의 구겨진 초대장",
                    "다니엘이 확인 중인 메신저 알림 살펴보기",
                    "다니엘과 대화",
                }));
            CharacterPlacement[] p01Placements = p01.CharacterSet.Placements;
            Assert.That(
                p01Placements
                    .Where(placement => placement.character != null)
                    .Select(placement => placement.character.Id),
                Is.EqualTo(new[] { "CHR_DANIEL" }));
            AssertPlacementSlot(
                p01Placements,
                "CHR_DANIEL",
                new Vector2(0.60f, 0.12f),
                0.78f,
                0);
            Assert.That(
                p01Placements.Single().clickable,
                Is.True,
                "P-01 must keep Daniel available for the authored messenger and dialogue interactions.");
            Assert.That(interactions[0].Action.GrantsEvidence, Is.True);
            Assert.That(interactions[0].HasWorldHotspot, Is.True);
            Assert.That(interactions[0].TargetId, Is.EqualTo("C-01"));
            Assert.That(
                interactions[0].NormalizedRect,
                Is.EqualTo(new Rect(0.012f, 0.182f, 0.066f, 0.086f)),
                "The invitation hotspot must stay aligned with the invitation painted on the authored P-01 background.");
            Assert.That(interactions.All(interaction => !interaction.Repeatable), Is.True);
            Assert.That(
                interactions.All(interaction => interaction.Type != InteractionType.Exit),
                Is.True,
                "A MapTravel route must not leave a world Exit hotspot in P-01.");
            Assert.That(
                interactions[2].Action,
                Is.TypeOf<DialogueInteractionAction>());
            Assert.That(
                ((DialogueInteractionAction)interactions[2].Action)
                    .AdvanceStorySceneOnComplete,
                Is.True,
                "Completing Daniel's authored dialogue must request the P-01 route.");

            AssertActionRange(interactions[0].Action, "P-01_003", "P-01_005");
            AssertActionRange(interactions[1].Action, "P-01_006", "P-01_008");
            AssertActionRange(interactions[2].Action, "P-01_009", "P-01_026");

            DialogueLine[] messengerLines = p01.EntryDialogue.Lines
                .Where(line => line.id is "P-01_006" or "P-01_007" or "P-01_008")
                .ToArray();
            Assert.That(
                messengerLines.Select(line => line.id),
                Is.EqualTo(new[] { "P-01_006", "P-01_007", "P-01_008" }));
            Assert.That(
                messengerLines.Select(line => line.voiceRequired),
                Is.EqualTo(new[] { false, true, false }),
                "The messenger range must preserve its narration/voiced/system contract.");
            Assert.That(messengerLines[0].text, Does.Contain("다니엘"));
            Assert.That(messengerLines[0].text, Does.Contain("기기"));

            Assert.That(interactions[0].IsAvailable(state), Is.True);
            Assert.That(interactions.Skip(1).All(item => !item.IsAvailable(state)), Is.True);
            Assert.That(
                director.TryGetFirstAvailableAnchored(
                    InteractionType.Context,
                    "CHR_DANIEL",
                    out _),
                Is.False);
            state.CompleteInteraction(interactions[0].Id);
            Assert.That(interactions[1].IsAvailable(state), Is.True);
            Assert.That(interactions.Skip(2).All(item => !item.IsAvailable(state)), Is.True);
            Assert.That(
                director.TryGetFirstAvailableAnchored(
                    InteractionType.Context,
                    "CHR_DANIEL",
                    out InteractionDefinition anchoredContext),
                Is.True);
            Assert.That(anchoredContext, Is.SameAs(interactions[1]));
            Assert.That(
                director.TryGetFirstAvailable(
                    InteractionType.Character,
                    "CHR_DANIEL",
                    out _),
                Is.False,
                "A Character request must not fall back to Daniel's pending Context interaction.");
            InteractionResult unavailableCharacter = director
                .ExecuteFirstAvailableAsync(InteractionType.Character, "CHR_DANIEL")
                .GetAwaiter()
                .GetResult();
            Assert.That(unavailableCharacter.Success, Is.False);
            Assert.That(state.IsInteractionCompleted(interactions[1].Id), Is.False);
            state.CompleteInteraction(interactions[1].Id);
            Assert.That(interactions[2].IsAvailable(state), Is.True);
            Assert.That(
                director.TryGetFirstAvailableAnchored(
                    InteractionType.Context,
                    "CHR_DANIEL",
                    out _),
                Is.False,
                "The anchored Context affordance must disappear once completed.");
            Assert.That(
                director.TryGetFirstAvailable(
                    InteractionType.Character,
                    "CHR_DANIEL",
                    out InteractionDefinition characterInteraction),
                Is.True);
            Assert.That(characterInteraction, Is.SameAs(interactions[2]));
            state.CompleteInteraction(interactions[2].Id);

            Assert.That(p01.Routes.Single().TargetSceneId, Is.EqualTo("P-02"));
            Assert.That(
                p01.Routes.Single().AdvanceMode,
                Is.EqualTo(StorySceneAdvanceMode.MapTravel));
            Assert.That(ConditionResolver.All(p02.EntryConditions, state), Is.False);
            state.CompleteScene("P-01");
            Assert.That(ConditionResolver.All(p02.EntryConditions, state), Is.True);

            Assert.That(p02.DeferEntryDialogue, Is.True);
            InteractionDefinition p02Dialogue = p02.InteractionSet.Interactions.Single();
            Assert.That(p02Dialogue.Id, Is.EqualTo("INT_P_02_DIALOGUE"));
            Assert.That(p02Dialogue.Type, Is.EqualTo(InteractionType.Character));
            Assert.That(p02Dialogue.TargetId, Is.EqualTo("CHR_DANIEL"));
            Assert.That(p02Dialogue.HasWorldHotspot, Is.False);
            Assert.That(p02Dialogue.Repeatable, Is.False);
            Assert.That(p02Dialogue.Action, Is.TypeOf<DialogueInteractionAction>());
            Assert.That(
                new SerializedObject(p02Dialogue.Action)
                    .FindProperty("dialogue")
                    .objectReferenceValue,
                Is.SameAs(p02.EntryDialogue));

            CharacterPlacement[] p02Placements = p02.CharacterSet.Placements;
            Assert.That(
                p02Placements
                    .Where(placement => placement.character != null)
                    .Select(placement => placement.character.Id),
                Is.EquivalentTo(new[]
                {
                    "CHR_EVELYN",
                    "CHR_DANIEL",
                    "CHR_RICHARD",
                }));
            // These coordinates use semantic slots from the current Gangway background;
            // the P-02 cast mapping is an authored layout that still needs visual approval.
            AssertPlacementSlot(
                p02Placements,
                "CHR_EVELYN",
                new Vector2(0.53f, 0.08f),
                0.78f,
                1);
            AssertPlacementSlot(
                p02Placements,
                "CHR_DANIEL",
                new Vector2(0.70f, 0.06f),
                0.78f,
                2);
            AssertPlacementSlot(
                p02Placements,
                "CHR_RICHARD",
                new Vector2(0.65f, 0.28f),
                0.63f,
                0);
            Assert.That(
                p02Placements.Single(
                    placement => placement.character?.Id == "CHR_DANIEL").clickable,
                Is.True,
                "P-02's deferred dialogue must have a present clickable Daniel target.");

            Assert.That(
                AssetDatabase.LoadAssetAtPath<InteractionDefinition>(
                    "Assets/_Project/Content/Locations/InteractionDefinitions/Generated/INT_P_01_CONTINUE.asset"),
                Is.Not.Null,
                "The retired P-01 exit asset must remain for GUID/save compatibility.");
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void P01TabletWarningPrecedesTheChoiceWhileLaterTrustBonusStillBranches()
    {
        DialogueSequence p01 = AssetDatabase.LoadAssetAtPath<DialogueSequence>(
            "Assets/_Project/Content/Dialogue/Prologue/DIA_P_01.asset");
        DialogueSequence p02 = AssetDatabase.LoadAssetAtPath<DialogueSequence>(
            "Assets/_Project/Content/Dialogue/Prologue/DIA_P_02.asset");
        DialogueLine tabletWarning = p01.Lines.Single(line => line.id == "P-01_018");
        DialogueLine choiceLine = p01.Lines.Single(line => line.id == "P-01_019");
        DialogueChoice seriousChoice = choiceLine.choices.Single(
            choice => choice.Id == "P-01_C1");
        DialogueChoice dismissiveChoice = choiceLine.choices.Single(
            choice => choice.Id == "P-01_C2");
        DialogueLine[] p02TrustBonus = p02.Lines
            .Where(line => line.id is "P-02_021" or "P-02_022")
            .ToArray();

        Assert.That(tabletWarning.voiceRequired, Is.True);
        Assert.That(tabletWarning.text, Does.Contain("예약 기사"));
        Assert.That(tabletWarning.text, Does.Contain("태블릿"));
        Assert.That(tabletWarning.conditions, Is.Null.Or.Empty);
        Assert.That(
            System.Array.FindIndex(p01.Lines, line => line.id == tabletWarning.id),
            Is.LessThan(System.Array.FindIndex(p01.Lines, line => line.id == choiceLine.id)),
            "The tablet warning must be heard before the player chooses how to answer Daniel.");
        Assert.That(p02TrustBonus, Has.Length.EqualTo(2));
        Assert.That(
            p02TrustBonus.All(line => line.conditions is { Length: > 0 }),
            Is.True,
            "Only the later P-02 exchange should remain gated by Daniel's trust.");

        var seriousOwner = new GameObject("P01SeriousTrustBranchTest");
        var dismissiveOwner = new GameObject("P01DismissiveTrustBranchTest");
        try
        {
            GameStateStore seriousState = seriousOwner.AddComponent<GameStateStore>();
            GameStateStore dismissiveState = dismissiveOwner.AddComponent<GameStateStore>();
            NarrativeDirector dismissiveNarrative =
                dismissiveOwner.AddComponent<NarrativeDirector>();
            var narrativeData = new SerializedObject(dismissiveNarrative);
            narrativeData.FindProperty("state").objectReferenceValue = dismissiveState;
            narrativeData.ApplyModifiedPropertiesWithoutUndo();
            dismissiveNarrative.LinePresented += line =>
                System.Threading.Tasks.Task.FromResult(
                    line.choices?.SingleOrDefault(
                        choice => choice.Id == dismissiveChoice.Id));

            Assert.That(seriousState.GetTrust("CHR_DANIEL"), Is.EqualTo(2));
            Assert.That(ConditionResolver.All(tabletWarning.conditions, seriousState), Is.True);
            Assert.That(
                p02TrustBonus.All(line => !ConditionResolver.All(line.conditions, seriousState)),
                Is.True);

            seriousChoice.Apply(seriousState);

            Assert.That(seriousState.GetTrust("CHR_DANIEL"), Is.EqualTo(3));
            Assert.That(seriousState.HasFlag("daniel_warning_taken"), Is.True);
            Assert.That(
                p02TrustBonus.All(line => ConditionResolver.All(line.conditions, seriousState)),
                Is.True,
                "Taking Daniel seriously should unlock the later trust bonus.");

            dismissiveNarrative.PlayAsync(
                    p01,
                    "P-01_009",
                    "P-01_026")
                .GetAwaiter()
                .GetResult();

            Assert.That(dismissiveState.GetTrust("CHR_DANIEL"), Is.EqualTo(1));
            Assert.That(dismissiveState.HasFlag("daniel_warning_dismissed"), Is.True);
            Assert.That(
                dismissiveNarrative.History.Lines,
                Does.Contain("P-01_018"),
                "The dismissive branch must still hear the core tablet warning.");

            int p02HistoryStart = dismissiveNarrative.History.Lines.Count;
            dismissiveState.CompleteScene("P-01");
            dismissiveNarrative.PlayAsync(p02).GetAwaiter().GetResult();
            string[] dismissiveP02History = dismissiveNarrative.History.Lines
                .Skip(p02HistoryStart)
                .ToArray();

            Assert.That(dismissiveP02History, Does.Contain("P-02_020"));
            Assert.That(dismissiveP02History, Does.Contain("P-02_023"));
            Assert.That(dismissiveP02History, Does.Not.Contain("P-02_021"));
            Assert.That(dismissiveP02History, Does.Not.Contain("P-02_022"));
            Assert.That(
                p02TrustBonus.All(line => !ConditionResolver.All(line.conditions, dismissiveState)),
                Is.True,
                "Dismissing Daniel should keep the later trust bonus hidden.");
        }
        finally
        {
            Object.DestroyImmediate(seriousOwner);
            Object.DestroyImmediate(dismissiveOwner);
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

    private static void AssertPlacementSlot(
        CharacterPlacement[] placements,
        string characterId,
        Vector2 expectedPosition,
        float expectedScale,
        int expectedSortingOrder)
    {
        CharacterPlacement placement = placements.Single(item =>
            item.character?.Id == characterId);
        Assert.That(
            new Vector2(placement.normalizedX, placement.normalizedY),
            Is.EqualTo(expectedPosition),
            $"{characterId} must remain in the authored placement slot.");
        Assert.That(placement.scale, Is.EqualTo(expectedScale), characterId);
        Assert.That(
            placement.sortingOrder,
            Is.EqualTo(expectedSortingOrder),
            characterId);
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

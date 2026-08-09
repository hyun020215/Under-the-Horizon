using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ContentValidator
{
    private static readonly HashSet<string> CanonicalStorySceneIds =
        BuildCanonicalStorySceneIds();

    private static readonly HashSet<string> CanonicalEvidenceIds =
        Enumerable.Range(1, 18)
            .Select(number => $"C-{number:00}")
            .ToHashSet(StringComparer.Ordinal);

    public static List<string> ValidateAll()
    {
        var errors = new List<string>();
        StorySceneDefinition[] scenes = LoadAll<StorySceneDefinition>();
        DialogueSequence[] dialogues = LoadAll<DialogueSequence>();
        EvidenceDefinition[] evidence = LoadAll<EvidenceDefinition>();
        PuzzleDefinition[] puzzles = LoadAll<PuzzleDefinition>();

        ValidateUniqueIds(scenes, item => item.Id, "Story Scene", errors);
        ValidateUniqueIds(LoadAll<LocationDefinition>(), item => item.Id, "Location", errors);
        ValidateUniqueIds(LoadAll<CharacterDefinition>(), item => item.Id, "Character", errors);
        ValidateUniqueIds(dialogues, item => item.Id, "Dialogue", errors);
        ValidateUniqueIds(evidence, item => item.Id, "Evidence", errors);
        ValidateUniqueIds(
            LoadAll<InteractionDefinition>(),
            item => item.Id,
            "Interaction",
            errors);
        ValidateUniqueIds(puzzles, item => item.Id, "Puzzle", errors);

        ValidateCanonicalIds(
            scenes.Select(item => item.Id),
            CanonicalStorySceneIds,
            "Story Scene",
            errors);
        ValidateCanonicalIds(
            evidence.Select(item => item.Id),
            CanonicalEvidenceIds,
            "Evidence",
            errors);

        ValidateStoryScenes(scenes, errors);
        ValidateLocations(errors);
        ValidateDialogues(dialogues, errors);
        ValidatePuzzles(puzzles, errors);
        ValidateDatabases(errors);
        ValidateGameDefinitions(errors);
        return errors;
    }

    private static void ValidateStoryScenes(
        StorySceneDefinition[] scenes,
        ICollection<string> errors)
    {
        var sceneIds = new HashSet<string>(
            scenes.Select(item => item.Id),
            StringComparer.Ordinal);

        foreach (StorySceneDefinition scene in scenes)
        {
            Require(scene, scene.Location, "Location", errors);
            Require(scene, scene.LocationState, "Location State", errors);
            Require(scene, scene.CharacterSet, "CharacterPlacementSet", errors);
            Require(scene, scene.InteractionSet, "InteractionSet", errors);
            Require(scene, scene.EntryDialogue, "Dialogue", errors);
            Require(scene, scene.AudioProfile, "Audio profile", errors);
            Require(scene, scene.EntryTransition, "entry Transition", errors);
            Require(scene, scene.ExitTransition, "exit Transition", errors);

            ValidatePlacements(scene, errors);
            ValidateInteractions(scene, errors);
            ValidateAuthoringRequirements(scene, errors);
            ValidateSequence(scene, scene.EntrySequence, "entry", errors);
            ValidateSequence(scene, scene.ExitSequence, "exit", errors);

            if (scene.OnCompleteEffects == null || scene.OnCompleteEffects.Length == 0)
                errors.Add($"{scene.Id} has no completion GameEffect.");

            if (scene.Routes == null)
                continue;

            foreach (StorySceneRoute route in scene.Routes)
            {
                if (route != null
                    && !string.IsNullOrWhiteSpace(route.TargetSceneId)
                    && !sceneIds.Contains(route.TargetSceneId))
                {
                    errors.Add(
                        $"{scene.Id} has a broken route to {route.TargetSceneId}.");
                }
            }
        }
    }

    private static void ValidatePlacements(
        StorySceneDefinition scene,
        ICollection<string> errors)
    {
        if (scene.CharacterSet?.Placements == null)
            return;

        foreach (CharacterPlacement placement in scene.CharacterSet.Placements)
        {
            if (placement.character == null)
                errors.Add($"{scene.Id} has a placement without a Character.");

            if (placement.normalizedX < 0f
                || placement.normalizedX > 1f
                || placement.normalizedY < 0f
                || placement.normalizedY > 1f)
            {
                errors.Add($"{scene.Id} has a placement outside normalized bounds.");
            }

            if (placement.scale <= 0f)
                errors.Add($"{scene.Id} has a placement with a non-positive scale.");
        }
    }

    private static void ValidateInteractions(
        StorySceneDefinition scene,
        ICollection<string> errors)
    {
        InteractionDefinition[] interactions = scene.InteractionSet?.Interactions;
        if (interactions == null || interactions.Length == 0)
        {
            errors.Add($"{scene.Id} has no authored interactions.");
            return;
        }

        foreach (InteractionDefinition interaction in interactions)
        {
            if (interaction == null || interaction.Action == null)
            {
                errors.Add($"{scene.Id} has an invalid interaction reference.");
                continue;
            }

            if (!interaction.HasWorldHotspot)
                continue;

            Rect rect = interaction.NormalizedRect;
            if (rect.width <= 0f
                || rect.height <= 0f
                || rect.xMin < 0f
                || rect.yMin < 0f
                || rect.xMax > 1f
                || rect.yMax > 1f)
            {
                errors.Add(
                    $"{scene.Id}/{interaction.Id} has an invalid normalized hotspot.");
            }
        }
    }

    private static void ValidateAuthoringRequirements(
        StorySceneDefinition scene,
        ICollection<string> errors)
    {
        StorySceneAuthoringRequirements requirements = scene.AuthoringRequirements;
        if (requirements == null)
            return;

        int interactionCount = scene.InteractionSet?.Interactions?.Length ?? 0;
        if (interactionCount < requirements.MinimumInteractionCount)
        {
            errors.Add(
                $"{scene.Id} requires at least "
                + $"{requirements.MinimumInteractionCount} interactions, "
                + $"but has {interactionCount}.");
        }

        if (requirements.RequiresPuzzle && scene.Puzzle == null)
            errors.Add($"{scene.Id} requires a Puzzle.");
        if (requirements.RequiresEntrySequence && scene.EntrySequence == null)
            errors.Add($"{scene.Id} requires an entry Sequence.");
        if (requirements.RequiresExitSequence && scene.ExitSequence == null)
            errors.Add($"{scene.Id} requires an exit Sequence.");

        InteractionDefinition[] interactions =
            scene.InteractionSet?.Interactions ?? Array.Empty<InteractionDefinition>();
        if (requirements.RequiredInteractionTypes != null)
        {
            foreach (InteractionType requiredType in
                     requirements.RequiredInteractionTypes.Distinct())
            {
                if (!interactions.Any(interaction =>
                        interaction != null && interaction.Type == requiredType))
                {
                    errors.Add(
                        $"{scene.Id} requires a {requiredType} interaction.");
                }
            }
        }

        if (requirements.RequiresEvidenceAcquisition
            && !interactions.Any(interaction =>
                interaction?.Action?.GrantsEvidence == true))
        {
            errors.Add($"{scene.Id} requires an evidence acquisition interaction.");
        }

        if (requirements.RequiresSceneChoice && !HasChoice(scene.EntryDialogue))
            errors.Add($"{scene.Id} requires a dialogue choice.");
    }

    private static void ValidateSequence(
        StorySceneDefinition scene,
        SceneSequenceDefinition sequence,
        string label,
        ICollection<string> errors)
    {
        if (sequence == null)
            return;

        if (sequence.Commands == null || sequence.Commands.Length == 0)
        {
            errors.Add($"{scene.Id} has an empty {label} Sequence.");
            return;
        }

        if (sequence.Commands.Any(command => command == null))
            errors.Add($"{scene.Id} has a null command in its {label} Sequence.");

        foreach (ImageMontageCommand montage in
                 sequence.Commands.OfType<ImageMontageCommand>())
        {
            if (montage.Frames == null || montage.Frames.Length == 0)
                errors.Add($"{scene.Id} has an empty image montage.");
            else if (montage.Frames.Any(frame => frame == null))
                errors.Add($"{scene.Id} has a missing image montage frame.");

            if (montage.HoldSeconds == null
                || montage.HoldSeconds.Length != montage.Frames?.Length)
            {
                errors.Add(
                    $"{scene.Id} image montage frame and hold counts differ.");
            }
        }

        if (scene.AuthoringRequirements?.RequiresEntrySequence == true
            && label == "entry"
            && sequence.Commands.All(command => command is WaitCommand))
        {
            errors.Add($"{scene.Id} has a placeholder-only entry Sequence.");
        }

        if (scene.AuthoringRequirements?.RequiresExitSequence == true
            && label == "exit"
            && sequence.Commands.All(command => command is WaitCommand))
        {
            errors.Add($"{scene.Id} has a placeholder-only exit Sequence.");
        }
    }

    private static void ValidateLocations(ICollection<string> errors)
    {
        foreach (LocationDefinition location in LoadAll<LocationDefinition>())
        {
            if (location.DefaultBackground == null)
                errors.Add($"{location.Id} has no default background.");
            if (location.DefaultAudio == null)
                errors.Add($"{location.Id} has no default audio profile.");

            if (location.States == null || location.States.Length == 0)
            {
                errors.Add($"{location.Id} has no Location State.");
                continue;
            }

            foreach (LocationStateDefinition state in location.States)
            {
                if (state == null || state.Background == null)
                    errors.Add($"{location.Id} has a State without a background.");
            }
        }
    }

    private static void ValidateDialogues(
        IEnumerable<DialogueSequence> dialogues,
        ICollection<string> errors)
    {
        foreach (DialogueSequence dialogue in dialogues)
        {
            DialogueLine[] lines = dialogue.Lines;
            if (lines == null || lines.Length == 0)
            {
                errors.Add($"{dialogue.Id} has no dialogue lines.");
                continue;
            }

            var lineIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (DialogueLine line in lines)
            {
                if (string.IsNullOrWhiteSpace(line.id))
                    errors.Add($"{dialogue.Id} has a line without an ID.");
                else if (!lineIds.Add(line.id))
                    errors.Add($"{dialogue.Id} has duplicate line ID {line.id}.");

                if (string.IsNullOrWhiteSpace(line.text))
                    errors.Add($"{dialogue.Id}/{line.id} has no text.");
            }

            foreach (DialogueLine line in lines)
                ValidateChoices(dialogue, line, lineIds, errors);
        }
    }

    private static void ValidateChoices(
        DialogueSequence dialogue,
        DialogueLine line,
        HashSet<string> lineIds,
        ICollection<string> errors)
    {
        if (line.choices == null)
            return;

        var choiceIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (DialogueChoice choice in line.choices)
        {
            if (choice == null)
            {
                errors.Add($"{dialogue.Id}/{line.id} has a null choice.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(choice.Id))
                errors.Add($"{dialogue.Id}/{line.id} has a choice without an ID.");
            else if (!choiceIds.Add(choice.Id))
                errors.Add($"{dialogue.Id}/{line.id} has duplicate choice {choice.Id}.");

            if (string.IsNullOrWhiteSpace(choice.Text))
                errors.Add($"{dialogue.Id}/{line.id}/{choice.Id} has no text.");

            if (!string.IsNullOrWhiteSpace(choice.NextLineId)
                && !lineIds.Contains(choice.NextLineId))
            {
                errors.Add(
                    $"{dialogue.Id}/{line.id}/{choice.Id} targets missing line "
                    + $"{choice.NextLineId}.");
            }
        }
    }

    private static void ValidatePuzzles(
        IEnumerable<PuzzleDefinition> puzzles,
        ICollection<string> errors)
    {
        HashSet<string> controllerKeys = TypeCache
            .GetTypesDerivedFrom<PuzzleControllerBase>()
            .Where(type => !type.IsAbstract)
            .Select(type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (PuzzleDefinition puzzle in puzzles)
        {
            if (string.IsNullOrWhiteSpace(puzzle.ControllerKey))
                errors.Add($"{puzzle.Id} has no controller key.");
            else if (!controllerKeys.Contains(puzzle.ControllerKey))
                errors.Add($"{puzzle.Id} has unknown controller {puzzle.ControllerKey}.");

            if (puzzle.CompletionEffects == null
                || puzzle.CompletionEffects.Length == 0)
            {
                errors.Add($"{puzzle.Id} has no completion GameEffect.");
            }
        }
    }

    private static void ValidateDatabases(ICollection<string> errors)
    {
        ContentDatabase[] databases = LoadAll<ContentDatabase>();
        if (databases.Length == 0)
        {
            errors.Add("No ContentDatabase exists.");
            return;
        }

        foreach (ContentDatabase database in databases)
        {
            ValidateDatabaseReferences(
                database.name,
                database.StoryScenes,
                CanonicalStorySceneIds,
                scene => scene?.Id,
                "Story Scene",
                errors);
            ValidateDatabaseReferences(
                database.name,
                database.Evidence,
                CanonicalEvidenceIds,
                item => item?.Id,
                "Evidence",
                errors);
        }
    }

    private static void ValidateDatabaseReferences<T>(
        string databaseName,
        IReadOnlyList<T> items,
        HashSet<string> expectedIds,
        Func<T, string> idSelector,
        string label,
        ICollection<string> errors)
        where T : UnityEngine.Object
    {
        if (items == null)
        {
            errors.Add($"{databaseName} has no {label} list.");
            return;
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (T item in items)
        {
            if (item == null)
                errors.Add($"{databaseName} has a null {label} reference.");
            else
                ids.Add(idSelector(item));
        }

        ValidateCanonicalIds(ids, expectedIds, $"{databaseName} {label}", errors);
    }

    private static void ValidateGameDefinitions(ICollection<string> errors)
    {
        GameDefinition[] games = LoadAll<GameDefinition>();
        if (games.Length == 0)
        {
            errors.Add("No GameDefinition exists.");
            return;
        }

        foreach (GameDefinition game in games)
        {
            if (game.Content == null)
                errors.Add($"{game.name} has no ContentDatabase.");
            else if (!game.Content.TryGetStoryScene(game.FirstStorySceneId, out _))
            {
                errors.Add(
                    $"{game.name} has an invalid first Story Scene "
                    + $"{game.FirstStorySceneId}.");
            }
        }
    }

    private static bool HasChoice(DialogueSequence dialogue) =>
        dialogue?.Lines?.Any(line => line.choices?.Length > 0) == true;

    private static void ValidateCanonicalIds(
        IEnumerable<string> actualIds,
        HashSet<string> expectedIds,
        string label,
        ICollection<string> errors)
    {
        var actual = new HashSet<string>(
            actualIds.Where(id => !string.IsNullOrWhiteSpace(id)),
            StringComparer.Ordinal);

        foreach (string missing in expectedIds.Except(actual).OrderBy(id => id))
            errors.Add($"Missing canonical {label} {missing}.");
        foreach (string unexpected in actual.Except(expectedIds).OrderBy(id => id))
            errors.Add($"Unexpected canonical {label} {unexpected}.");
    }

    private static HashSet<string> BuildCanonicalStorySceneIds()
    {
        var ids = new HashSet<string>(
            new[] { "P-01", "P-02", "P-03" },
            StringComparer.Ordinal);
        int[] scenesPerDay = { 7, 6, 5, 4, 4, 5, 4, 3 };

        for (var day = 1; day <= scenesPerDay.Length; day++)
        {
            for (var scene = 1; scene <= scenesPerDay[day - 1]; scene++)
                ids.Add($"D{day}-{scene:00}");
        }

        return ids;
    }

    private static void Require(
        StorySceneDefinition scene,
        UnityEngine.Object value,
        string label,
        ICollection<string> errors)
    {
        if (value == null)
            errors.Add($"{scene.Id} has no {label}.");
    }

    private static void ValidateUniqueIds<T>(
        IEnumerable<T> assets,
        Func<T, string> idSelector,
        string label,
        ICollection<string> errors)
        where T : UnityEngine.Object
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (T asset in assets)
        {
            string id = idSelector(asset);
            if (string.IsNullOrWhiteSpace(id))
                errors.Add($"{label} has no ID: {AssetDatabase.GetAssetPath(asset)}");
            else if (!ids.Add(id))
                errors.Add($"Duplicate {label} ID: {id}");
        }
    }

    private static T[] LoadAll<T>() where T : UnityEngine.Object =>
        AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(item => item != null)
            .ToArray();
}

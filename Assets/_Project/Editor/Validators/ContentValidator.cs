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
        MapDefinition[] maps = LoadAll<MapDefinition>();

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
        ValidateUniqueIds(maps, item => item.Id, "Map", errors);

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
        ValidateMaps(maps, scenes, errors);
        ValidateAudioCueProfiles(errors);
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

        var placedCharacterIds = new HashSet<string>(
            (scene.CharacterSet?.Placements ?? Array.Empty<CharacterPlacement>())
                .Where(placement => placement.character != null)
                .Select(placement => placement.character.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id)),
            StringComparer.Ordinal);
        var clickableCharacterIds = new HashSet<string>(
            (scene.CharacterSet?.Placements ?? Array.Empty<CharacterPlacement>())
                .Where(placement => placement.character != null && placement.clickable)
                .Select(placement => placement.character.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id)),
            StringComparer.Ordinal);
        var attachedContextDefinitions = new HashSet<InteractionDefinition>();

        foreach (InteractionDefinition interaction in interactions)
        {
            if (interaction == null)
            {
                errors.Add($"{scene.Id} has an invalid interaction reference.");
                continue;
            }

            bool isCharacterAttachedContext = interaction.Type == InteractionType.Context
                && !interaction.HasWorldHotspot
                && !string.IsNullOrWhiteSpace(interaction.TargetId);
            if (isCharacterAttachedContext)
            {
                if (!attachedContextDefinitions.Add(interaction))
                {
                    errors.Add(
                        $"{scene.Id}/{interaction.Id} repeats the same "
                        + "character-attached Context definition.");
                }

                if (string.IsNullOrWhiteSpace(interaction.DisplayName))
                {
                    errors.Add(
                        $"{scene.Id}/{interaction.Id} is a character-attached Context "
                        + "without a display name.");
                }

                if (!placedCharacterIds.Contains(interaction.TargetId))
                {
                    errors.Add(
                        $"{scene.Id}/{interaction.Id} targets {interaction.TargetId}, "
                        + "which is not present in its CharacterPlacementSet.");
                }
            }

            if (interaction.Action == null)
            {
                errors.Add($"{scene.Id} has an invalid interaction reference.");
                continue;
            }

            if (!interaction.HasWorldHotspot)
                continue;

            if (string.IsNullOrWhiteSpace(interaction.DisplayName))
            {
                errors.Add(
                    $"{scene.Id}/{interaction.Id} is a world hotspot without "
                    + "a display name for its tooltip.");
            }

            Rect rect = interaction.NormalizedRect;
            if (rect.width <= 0f
                || rect.height <= 0f
                || rect.xMin < 0f
                || rect.yMin < 0f
                || rect.xMax > 1f
                || rect.yMax > 1f)
            {
                errors.Add(
                    $"{scene.Id}/{interaction.Id} has an invalid normalized hotspot "
                    + $"{rect}.");
            }
        }

        if (scene.DeferEntryDialogue
            && !interactions.Any(interaction =>
                CanExecuteDeferredEntryDialogue(
                    scene,
                    interaction,
                    clickableCharacterIds)))
        {
            errors.Add(
                $"{scene.Id} defers its entry Dialogue but has no executable "
                + "interaction for that Dialogue on a present clickable target.");
        }

        InteractionDefinition[] advanceInteractions = interactions
            .Where(AdvancesStoryScene)
            .ToArray();
        if (advanceInteractions.Length > 1)
        {
            errors.Add(
                $"{scene.Id} has more than one Story Scene advance interaction.");
        }

        foreach (InteractionDefinition interaction in advanceInteractions)
        {
            if (interaction.Repeatable)
            {
                errors.Add(
                    $"{scene.Id}/{interaction.Id} advances the Story Scene "
                    + "but is repeatable.");
            }
        }

        if (advanceInteractions.Length > 0
            && (scene.Routes == null
                || !scene.Routes.Any(route =>
                    route != null
                    && !string.IsNullOrWhiteSpace(route.TargetSceneId))))
        {
            errors.Add(
                $"{scene.Id} has a Story Scene advance interaction but no route.");
        }

        bool hasMapTravelRoute = scene.Routes?.Any(route =>
            route != null
            && route.AdvanceMode == StorySceneAdvanceMode.MapTravel) == true;
        if (!hasMapTravelRoute)
            return;

        foreach (InteractionDefinition exit in interactions.Where(interaction =>
                     interaction != null
                     && interaction.Type == InteractionType.Exit
                     && interaction.HasWorldHotspot))
        {
            errors.Add(
                $"{scene.Id}/{exit.Id} is a world Exit hotspot on a MapTravel "
                + "source; map travel must be requested by an authored action "
                + "and confirmed on the Map screen.");
        }

        if (advanceInteractions.Length == 0)
        {
            errors.Add(
                $"{scene.Id} has a MapTravel route but no interaction requests "
                + "Story Scene advancement.");
        }
    }

    private static bool CanExecuteDeferredEntryDialogue(
        StorySceneDefinition scene,
        InteractionDefinition interaction,
        ISet<string> clickableCharacterIds)
    {
        if (interaction?.Action == null)
            return false;

        SerializedProperty dialogueProperty = new SerializedObject(interaction.Action)
            .FindProperty("dialogue");
        if (dialogueProperty?.objectReferenceValue != scene.EntryDialogue)
            return false;

        if (interaction.HasWorldHotspot)
            return true;

        return (interaction.Type == InteractionType.Character
                || interaction.Type == InteractionType.Context)
            && !string.IsNullOrWhiteSpace(interaction.TargetId)
            && clickableCharacterIds.Contains(interaction.TargetId);
    }

    private static bool AdvancesStoryScene(InteractionDefinition interaction) =>
        interaction?.Action is StorySceneAdvanceInteractionAction
        || interaction?.Action is DialogueInteractionAction
        {
            AdvanceStorySceneOnComplete: true
        };

    private static void ValidateAuthoringRequirements(
        StorySceneDefinition scene,
        ICollection<string> errors)
    {
        StorySceneAuthoringRequirements requirements = scene.AuthoringRequirements;
        if (requirements == null)
            return;

        int interactionCount = scene.InteractionSet?.Interactions?.Length ?? 0;
        InteractionDefinition[] interactions =
            scene.InteractionSet?.Interactions ?? Array.Empty<InteractionDefinition>();
        if (interactionCount < requirements.MinimumInteractionCount)
        {
            errors.Add(
                $"{scene.Id} requires at least "
                + $"{requirements.MinimumInteractionCount} interactions, "
                + $"but has {interactionCount}.");
        }

        if (requirements.RequiresPuzzle && scene.Puzzle == null)
            errors.Add($"{scene.Id} requires a Puzzle.");
        else if (requirements.RequiresPuzzle
            && !interactions.Any(interaction =>
                interaction?.Action is PuzzleInteractionAction action
                && action.Puzzle == scene.Puzzle))
        {
            errors.Add(
                $"{scene.Id} has no interaction for its assigned Puzzle.");
        }
        if (requirements.RequiresEntrySequence && scene.EntrySequence == null)
            errors.Add($"{scene.Id} requires an entry Sequence.");
        if (requirements.RequiresExitSequence && scene.ExitSequence == null)
            errors.Add($"{scene.Id} requires an exit Sequence.");

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

    private static void ValidateMaps(
        MapDefinition[] maps,
        StorySceneDefinition[] scenes,
        ICollection<string> errors)
    {
        var mapCountByLocationId = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (MapDefinition map in maps)
        {
            string authoredDisplayName = new SerializedObject(map)
                .FindProperty("displayName")
                .stringValue;
            if (string.IsNullOrWhiteSpace(authoredDisplayName)
                || authoredDisplayName.Contains("MAP_", StringComparison.Ordinal))
            {
                errors.Add($"{map.Id} has no player-facing map display name.");
            }

            if (map.BaseLayer == null)
                errors.Add($"{map.Id} has no base map layer.");

            if (map.Locations == null || map.Locations.Length == 0)
            {
                errors.Add($"{map.Id} has no authored Location nodes.");
                continue;
            }

            var locationIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (LocationDefinition location in map.Locations)
            {
                if (location == null)
                {
                    errors.Add($"{map.Id} has an invalid Location reference.");
                    continue;
                }

                if (!locationIds.Add(location.Id))
                    errors.Add($"{map.Id} repeats Location {location.Id}.");

                mapCountByLocationId.TryGetValue(location.Id, out int currentMapCount);
                mapCountByLocationId[location.Id] = currentMapCount + 1;

                MapNodeDefinition node = location.MapNode;
                if (node == null)
                {
                    errors.Add($"{map.Id}/{location.Id} has no Map node.");
                    continue;
                }

                if (!string.Equals(node.Id, location.Id, StringComparison.Ordinal))
                {
                    errors.Add(
                        $"{map.Id}/{location.Id} has mismatched Map node ID "
                        + $"'{node.Id}'.");
                }

                Vector2 position = node.NormalizedPosition;
                if (position.x < 0f || position.x > 1f
                    || position.y < 0f || position.y > 1f)
                {
                    errors.Add(
                        $"{map.Id}/{location.Id} has a Map node outside "
                        + "normalized bounds.");
                }

                string effectiveNodeName = !string.IsNullOrWhiteSpace(node.DisplayName)
                    ? node.DisplayName
                    : location.DisplayName;
                if (string.IsNullOrWhiteSpace(effectiveNodeName)
                    || effectiveNodeName.Contains("LOC_", StringComparison.Ordinal))
                {
                    errors.Add(
                        $"{map.Id}/{location.Id} has no player-facing Map node name.");
                }

                if (node.AccessMode == MapNodeAccessMode.RouteOnly
                    && string.IsNullOrWhiteSpace(node.DisplayName))
                {
                    errors.Add(
                        $"{map.Id}/{location.Id} is RouteOnly but has no explicit "
                        + "Map node name.");
                }

                if (node.AccessMode == MapNodeAccessMode.RouteOnly
                    && string.IsNullOrWhiteSpace(node.Description))
                {
                    errors.Add(
                        $"{map.Id}/{location.Id} is RouteOnly but has no player-facing "
                        + "Map node description.");
                }
            }
        }

        foreach (StorySceneDefinition source in scenes)
        {
            foreach (StorySceneRoute route in source.Routes ?? Array.Empty<StorySceneRoute>())
            {
                if (route == null || route.AdvanceMode != StorySceneAdvanceMode.MapTravel)
                    continue;

                StorySceneDefinition target = scenes.FirstOrDefault(scene =>
                    scene != null
                    && string.Equals(
                        scene.Id,
                        route.TargetSceneId,
                        StringComparison.Ordinal));
                if (target?.Location == null)
                    continue;

                if (!mapCountByLocationId.TryGetValue(target.Location.Id, out int mapCount)
                    || mapCount == 0)
                {
                    errors.Add(
                        $"{source.Id} MapTravel destination {target.Location.Id} "
                        + "is not represented on a MapDefinition.");
                }
                else if (mapCount > 1)
                {
                    errors.Add(
                        $"{source.Id} MapTravel destination {target.Location.Id} "
                        + "is represented on more than one MapDefinition.");
                }
            }
        }
    }

    private static void ValidateAudioCueProfiles(ICollection<string> errors)
    {
        foreach (AudioCueProfile profile in LoadAll<AudioCueProfile>())
        {
            ValidateAudioRole(
                profile,
                profile.music,
                "music",
                "Music",
                "MUS_",
                errors);
            ValidateAudioRole(
                profile,
                profile.ambienceA,
                "ambience A",
                "Ambience",
                "AMB_",
                errors);
            ValidateAudioRole(
                profile,
                profile.ambienceB,
                "ambience B",
                "Ambience",
                "AMB_",
                errors);
            ValidateAudioRole(
                profile,
                profile.entryStinger,
                "entry stinger",
                "SFX",
                "SFX_",
                errors);
        }
    }

    private static void ValidateAudioRole(
        AudioCueProfile profile,
        AudioClip clip,
        string slot,
        string roleFolder,
        string prefix,
        ICollection<string> errors)
    {
        if (clip == null)
            return;

        string path = AssetDatabase.GetAssetPath(clip).Replace('\\', '/');
        string expectedRoot = $"Assets/_Project/Audio/{roleFolder}/";
        if (!path.StartsWith(expectedRoot, StringComparison.Ordinal)
            || !clip.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(
                $"{profile.name} {slot} must reference "
                + $"{expectedRoot}{prefix}*, but references {path}.");
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

            if (puzzle.Rules?.IsAuthored == true)
            {
                HashSet<string> allowed = puzzle.Rules.AllowedInputIds
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToHashSet(StringComparer.Ordinal);
                if (puzzle.Rules.SolutionIds.Length == 0)
                    errors.Add($"{puzzle.Id} has an empty authored solution.");
                foreach (string id in puzzle.Rules.SolutionIds)
                    if (!allowed.Contains(id))
                        errors.Add($"{puzzle.Id} solution '{id}' is not an allowed input.");
                foreach (string id in puzzle.Rules.RequiredEvidenceIds)
                    if (!CanonicalEvidenceIds.Contains(id))
                        errors.Add($"{puzzle.Id} requires unknown evidence {id}.");
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

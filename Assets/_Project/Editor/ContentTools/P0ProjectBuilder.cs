using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class P0ProjectBuilder
{
    private const string ProjectRoot = "Assets/_Project";
    private const string ContentRoot = ProjectRoot + "/Content";
    private const string PrefabRoot = ProjectRoot + "/Prefabs";
    private const string SceneIndexPath = ContentRoot + "/StoryScenes/StoryScene_Index_KR.csv";
    private const string DialoguePath = ContentRoot + "/Dialogue/Source/Dialogue_Master_KR.csv";
    private const string EvidencePath = ContentRoot + "/Evidence/Evidence_Master_KR.csv";
    private const string ThemePanelPath = ProjectRoot + "/Art/UI/Panels/UI_panel_narrative_frame.png";

    private sealed class SceneRow
    {
        public string Id;
        public string Chapter;
        public string Title;
        public string Time;
        public string Location;
        public string Next;
        public string Characters;
    }

    [MenuItem("Under The Horizon/Build/P0 Project Content")]
    public static void BuildAll()
    {
        ConfigureTexture(ThemePanelPath, true);
        ConfigureRuntimeVisualQuality();
        CreatePlaceholderAssets();

        List<SceneRow> scenes = ReadSceneRows();
        BuildLocationDefaults(scenes);
        BuildUiAudio();
        BuildDialogue(scenes);
        DialogueCsvImporter.ImportAll();
        BuildSceneSupportAssets(scenes);
        PopulateEvidence();
        PopulateCharacters();
        PopulateMaps();
        PopulateStoryScenes(scenes);
        PuzzleInteractionContentMigrator.MigrateAll();
        PopulateDatabases();
        BuildPrefabs();
        BuildScenes();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"P0 project build complete: {scenes.Count} Story Scenes.");
    }

    public static void BuildFromCommandLine()
    {
        BuildAll();
        EditorApplication.Exit(0);
    }

    public static void RefreshPresentationFromCommandLine()
    {
        ConfigureRuntimeVisualQuality();
        BuildPresentationPrefabs();
        BuildGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    public static void RefreshSettingsFromCommandLine()
    {
        Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        SavePrefab(
            PrefabRoot + "/UI/PF_SettingsScreen.prefab",
            CreateScreen("PF_SettingsScreen", typeof(SettingsScreen), ScreenId.Settings, panel));
        BuildGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    public static void RefreshGameSceneFromCommandLine()
    {
        BuildGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    public static void RefreshPresentationProfilesFromCommandLine()
    {
        EnsurePresentationProfiles();
        BuildPresentationPrefabs();
        BuildGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    public static void RefreshSaveSlotFromCommandLine()
    {
        Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        SavePrefab(PrefabRoot + "/UI/PF_SaveSlotScreen.prefab",
            CreateScreen("PF_SaveSlotScreen", typeof(SaveSlotScreen), ScreenId.SaveSlot, panel));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    public static void RefreshMapFromCommandLine()
    {
        Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        SavePrefab(PrefabRoot + "/UI/PF_MapScreen.prefab",
            CreateScreen("PF_MapScreen", typeof(MapScreen), ScreenId.Map, panel));
        BuildGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    public static void RefreshInvestigationRecordFromCommandLine()
    {
        Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        SavePrefab(PrefabRoot + "/UI/PF_RecordScreen.prefab",
            CreateScreen("PF_RecordScreen", typeof(InvestigationRecordScreen),
                ScreenId.InvestigationRecord, panel));
        BuildGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    public static void RefreshDialogueFromCommandLine()
    {
        Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        SavePrefab(PrefabRoot + "/UI/PF_DialogueScreen.prefab",
            CreateScreen("PF_DialogueScreen", typeof(DialogueScreen),
                ScreenId.Dialogue, panel));
        BuildGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    public static void RefreshEvidenceBoardFromCommandLine()
    {
        Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        SavePrefab(PrefabRoot + "/UI/PF_EvidenceBoardScreen.prefab",
            CreateScreen("PF_EvidenceBoardScreen", typeof(EvidenceBoardScreen), ScreenId.EvidenceBoard, panel));
        SavePrefab(PrefabRoot + "/UI/PF_RecordScreen.prefab",
            CreateScreen("PF_RecordScreen", typeof(InvestigationRecordScreen), ScreenId.InvestigationRecord, panel));
        BuildGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    public static void RefreshPuzzleShellFromCommandLine()
    {
        Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        SavePrefab(PrefabRoot + "/UI/PF_PuzzleScreen.prefab",
            CreateScreen("PF_PuzzleScreen", typeof(PuzzleScreen), ScreenId.Puzzle, panel));
        BuildGameScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    private static void EnsurePresentationProfiles()
    {
        EnsureAsset<AmbientParticleProfile>(
            ContentRoot + "/UI/UI_AMBIENCE_TITLE.asset");
        EnsureAsset<AmbientParticleProfile>(
            ContentRoot + "/UI/UI_AMBIENCE_LOCATION.asset");
        EnsureAsset<CharacterPresentationProfile>(
            ContentRoot + "/Characters/CHR_PRESENTATION_STANDARD.asset");
    }

    private static void EnsureAsset<T>(string path) where T : ScriptableObject
    {
        if (AssetDatabase.LoadAssetAtPath<T>(path) != null)
            return;
        T asset = ScriptableObject.CreateInstance<T>();
        asset.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(asset, path);
    }

    private static void CreatePlaceholderAssets()
    {
        string absoluteContent = Path.GetFullPath(ContentRoot);
        foreach (string absolutePath in Directory.GetFiles(absoluteContent, "*.asset", SearchOption.AllDirectories))
        {
            if (new FileInfo(absolutePath).Length > 0)
                continue;

            string path = ToAssetPath(absolutePath);
            Type type = ResolveAssetType(path);
            if (type == null)
                throw new InvalidOperationException($"No P0 asset type mapping for '{path}'.");

            ReplaceAsset(path, ScriptableObject.CreateInstance(type));
        }
    }

    private static Type ResolveAssetType(string path)
    {
        if (path.Contains("/Audio/Ducking/"))
            return typeof(DuckingProfile);
        if (path.Contains("/Audio/"))
            return typeof(AudioCueProfile);
        if (path.Contains("/Characters/PlacementSets/"))
            return typeof(CharacterPlacementSet);
        if (path.Contains("/Characters/"))
            return typeof(CharacterDefinition);
        if (path.Contains("/Dialogue/") && Path.GetFileName(path).StartsWith("BARK_"))
            return typeof(DialogueSequence);
        if (path.Contains("/Dialogue/"))
            return typeof(DialogueSequence);
        if (path.Contains("/Evidence/"))
            return typeof(EvidenceDefinition);
        if (path.EndsWith("/DATABASE_Content.asset"))
            return typeof(ContentDatabase);
        if (path.EndsWith("/DATABASE_Dialogue.asset"))
            return typeof(DialogueDatabase);
        if (path.EndsWith("/DATABASE_Evidence.asset"))
            return typeof(EvidenceDatabase);
        if (path.Contains("/Game/") && Path.GetFileName(path).StartsWith("DATABASE_"))
            return typeof(ContentCatalog);
        if (path.Contains("/Game/") && Path.GetFileName(path).StartsWith("GAME_"))
            return typeof(GameDefinition);
        if (path.Contains("/Locations/Definitions/"))
            return typeof(LocationDefinition);
        if (path.Contains("/Locations/InteractionSets/"))
            return typeof(InteractionSet);
        if (path.Contains("/Locations/Map/"))
            return typeof(MapDefinition);
        if (path.Contains("/Locations/States/"))
            return typeof(LocationStateDefinition);
        if (path.Contains("/Puzzles/"))
            return typeof(PuzzleDefinition);
        if (path.Contains("/Sequences/"))
            return typeof(SceneSequenceDefinition);
        if (path.Contains("/StoryScenes/"))
            return typeof(StorySceneDefinition);
        if (path.Contains("/Theories/"))
            return typeof(TheoryDefinition);
        if (path.Contains("/Transitions/"))
            return typeof(TransitionProfile);
        if (path.Contains("/UI/"))
            return typeof(UIThemeProfile);
        return null;
    }

    private static void BuildLocationDefaults(IReadOnlyList<SceneRow> scenes)
    {
        EnsureFolder(ContentRoot + "/Locations/States/Generated");
        foreach (LocationDefinition location in LoadAll<LocationDefinition>())
        {
            string token = location.name.Replace("LOC_", string.Empty);
            SetString(location, "id", location.name);
            SetString(location, "displayName", Humanize(token));
            SetObject(location, "defaultBackground", FindLocationSprite(token));

            string statePath = $"{ContentRoot}/Locations/States/Generated/{token}_Default.asset";
            LocationStateDefinition state = GetOrCreate<LocationStateDefinition>(statePath);
            SetString(state, "id", token + "_DEFAULT");
            SetObject(state, "background", FindLocationSprite(token));

            string audioPath = $"{ContentRoot}/Audio/LocationDefaults/AUDIO_{token}_DEFAULT.asset";
            AudioCueProfile audio = GetOrCreate<AudioCueProfile>(audioPath);
            ConfigureAudio(audio, token);
            SetObject(location, "defaultAudio", audio);
            UnityEngine.Object[] states = (location.States ?? Array.Empty<LocationStateDefinition>())
                .Where(item => item != null)
                .Cast<UnityEngine.Object>()
                .Append(state)
                .Distinct()
                .ToArray();
            SetArray(location, "states", states);
        }
    }

    private static void BuildUiAudio()
    {
        AudioCueProfile title = GetOrCreate<AudioCueProfile>(
            ContentRoot + "/Audio/AUDIO_TITLE.asset"
        );
        title.music = FindAudio("Mystery");
        title.musicVolume = 0.65f;
        title.ambienceA = null;
        title.ambienceB = null;
        title.ambienceAVolume = 0f;
        title.ambienceBVolume = 0f;
        title.crossfadeDuration = 1.2f;
        EditorUtility.SetDirty(title);
    }

    private static void BuildDialogue(IReadOnlyList<SceneRow> scenes)
    {
        Dictionary<string, List<List<string>>> grouped = ReadCsv(DialoguePath)
            .Skip(1)
            .Where(row => row.Count > 6 && !string.IsNullOrWhiteSpace(row[1]))
            .GroupBy(row => row[1])
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (SceneRow scene in scenes)
        {
            string path = DialogueAssetPath(scene.Id);
            DialogueSequence sequence = GetOrCreate<DialogueSequence>(path);
            SerializedObject serialized = new(sequence);
            serialized.FindProperty("id").stringValue = "DIA_" + NormalizeId(scene.Id);
            SerializedProperty lines = serialized.FindProperty("lines");
            List<List<string>> source = grouped.TryGetValue(scene.Id, out var rows) ? rows : new();
            lines.arraySize = source.Count;
            for (int index = 0; index < source.Count; index++)
            {
                List<string> row = source[index];
                SerializedProperty line = lines.GetArrayElementAtIndex(index);
                line.FindPropertyRelative("id").stringValue = row[0];
                line.FindPropertyRelative("text").stringValue = row[6];
                line.FindPropertyRelative("voiceRequired").boolValue =
                    row.Count > 12 && row[12].Equals("Y", StringComparison.OrdinalIgnoreCase);
                SerializedProperty speaker = line.FindPropertyRelative("speaker");
                speaker.FindPropertyRelative("overrideName").stringValue = row.Count > 5 ? row[5] : string.Empty;
                speaker.FindPropertyRelative("character").objectReferenceValue = FindCharacter(row.Count > 5 ? row[5] : string.Empty);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sequence);
        }

        foreach (DialogueSequence sequence in LoadAll<DialogueSequence>())
            if (string.IsNullOrWhiteSpace(sequence.Id))
                SetString(sequence, "id", sequence.name);
    }

    private static void BuildSceneSupportAssets(IReadOnlyList<SceneRow> scenes)
    {
        const string legacyInteractionFolder = ContentRoot + "/Locations/InteractionS";
        const string interactionFolder = ContentRoot + "/Locations/InteractionDefinitions";
        if (AssetDatabase.IsValidFolder(legacyInteractionFolder)
            && !AssetDatabase.IsValidFolder(interactionFolder))
        {
            string moveError = AssetDatabase.MoveAsset(legacyInteractionFolder, interactionFolder);
            if (!string.IsNullOrEmpty(moveError))
                throw new InvalidOperationException(moveError);
        }
        EnsureFolder(ContentRoot + "/Characters/PlacementSets/Generated");
        EnsureFolder(ContentRoot + "/Locations/InteractionSets/Generated");
        EnsureFolder(interactionFolder + "/Generated");
        EnsureFolder(ContentRoot + "/Effects/Generated");

        foreach (SceneRow scene in scenes)
        {
            CharacterPlacementSet placements = GetOrCreate<CharacterPlacementSet>(PlacementPath(scene.Id));
            CharacterDefinition[] sceneCharacters = SplitCharacters(scene.Characters)
                .Select(FindCharacter)
                .Where(character => character != null)
                .Distinct()
                .ToArray();
            SerializedObject placementObject = new(placements);
            SerializedProperty items = placementObject.FindProperty("placements");
            items.arraySize = sceneCharacters.Length;
            for (int index = 0; index < sceneCharacters.Length; index++)
            {
                SerializedProperty item = items.GetArrayElementAtIndex(index);
                item.FindPropertyRelative("character").objectReferenceValue = sceneCharacters[index];
                item.FindPropertyRelative("normalizedX").floatValue =
                    sceneCharacters.Length == 1 ? 0.78f : 0.58f + 0.34f * index / (sceneCharacters.Length - 1f);
                item.FindPropertyRelative("normalizedY").floatValue = 0.04f;
                item.FindPropertyRelative("scale").floatValue = sceneCharacters.Length > 2 ? 0.78f : 0.94f;
                item.FindPropertyRelative("sortingOrder").intValue = index;
                item.FindPropertyRelative("clickable").boolValue = true;
            }
            placementObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(placements);

            InteractionSet interactions = GetOrCreate<InteractionSet>(InteractionPath(scene.Id));
            if (interactions.Interactions != null && interactions.Interactions.Length > 0)
                continue;

            DialogueSequence dialogue = AssetDatabase.LoadAssetAtPath<DialogueSequence>(
                DialogueAssetPath(scene.Id)
            );
            string interactionRoot = interactionFolder + "/Generated/";
            DialogueInteractionAction action = GetOrCreate<DialogueInteractionAction>(
                interactionRoot + $"ACT_{NormalizeId(scene.Id)}_DIALOGUE.asset"
            );
            SetObject(action, "dialogue", dialogue);
            InteractionDefinition interaction = GetOrCreate<InteractionDefinition>(
                interactionRoot + $"INT_{NormalizeId(scene.Id)}_DIALOGUE.asset"
            );
            SetString(interaction, "id", $"INT_{NormalizeId(scene.Id)}_DIALOGUE");
            SetEnum(interaction, "type", (int)InteractionType.Context);
            SetObject(interaction, "action", action);
            SetBool(interaction, "repeatable", true);
            SetArray(interactions, "interactions", new UnityEngine.Object[] { interaction });
        }
    }

    private static void PopulateStoryScenes(IReadOnlyList<SceneRow> scenes)
    {
        Dictionary<string, StorySceneDefinition> definitions = LoadAll<StorySceneDefinition>()
            .ToDictionary(item => NormalizeIdFromName(item.name), StringComparer.OrdinalIgnoreCase);

        foreach (SceneRow row in scenes)
        {
            if (!definitions.TryGetValue(NormalizeId(row.Id), out StorySceneDefinition definition))
                throw new InvalidOperationException($"Story Scene asset missing for {row.Id}.");

            int day = ParseDay(row.Id);
            SerializedObject serialized = new(definition);
            serialized.FindProperty("id").stringValue = row.Id;
            serialized.FindProperty("displayName").stringValue = row.Title;
            serialized.FindProperty("chapter").enumValueIndex = day;
            serialized.FindProperty("day").enumValueIndex = day;
            serialized.FindProperty("timeBlock").enumValueIndex = ParseTimeBlock(row.Time);
            serialized.FindProperty("initialScreen").enumValueIndex = (int)ScreenMode.Exploration;

            LocationDefinition location = FindLocation(row.Location);
            serialized.FindProperty("location").objectReferenceValue = location;
            SerializedProperty locationState = serialized.FindProperty("locationState");
            if (locationState.objectReferenceValue == null)
                locationState.objectReferenceValue = location?.States?.FirstOrDefault();
            serialized.FindProperty("characterSet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<CharacterPlacementSet>(PlacementPath(row.Id));
            serialized.FindProperty("interactionSet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<InteractionSet>(InteractionPath(row.Id));
            serialized.FindProperty("entryDialogue").objectReferenceValue = AssetDatabase.LoadAssetAtPath<DialogueSequence>(DialogueAssetPath(row.Id));
            serialized.FindProperty("audioProfile").objectReferenceValue = location?.DefaultAudio;
            serialized.FindProperty("entryTransition").objectReferenceValue = FindTransition("TRANS_FADE_STANDARD");
            serialized.FindProperty("exitTransition").objectReferenceValue = FindTransition("TRANS_FADE_STANDARD");

            string next = FirstNextScene(row.Next);
            SerializedProperty routes = serialized.FindProperty("routes");
            routes.arraySize = string.IsNullOrWhiteSpace(next) ? 0 : 1;
            if (routes.arraySize == 1)
                routes.GetArrayElementAtIndex(0).FindPropertyRelative("targetSceneId").stringValue = next;

            PuzzleDefinition puzzle = FindPuzzleForScene(row.Id);
            serialized.FindProperty("puzzle").objectReferenceValue = puzzle;
            CompleteSceneEffect complete = GetOrCreate<CompleteSceneEffect>(
                $"{ContentRoot}/Effects/Generated/FX_{NormalizeId(row.Id)}_COMPLETE.asset"
            );
            SetString(complete, "sceneId", row.Id);
            SerializedProperty completeEffects = serialized.FindProperty("onCompleteEffects");
            completeEffects.arraySize = 1;
            completeEffects.GetArrayElementAtIndex(0).objectReferenceValue = complete;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }
    }

    private static void PopulateCharacters()
    {
        foreach (CharacterDefinition character in LoadAll<CharacterDefinition>())
        {
            string token = character.name.Replace("CHR_", string.Empty);
            SetString(character, "id", character.name);
            SetString(character, "displayName", Humanize(token));
            SetObject(character, "portrait", FindCharacterSprite(token));
        }
    }

    private static void PopulateEvidence()
    {
        Dictionary<string, List<string>> rows = ReadCsv(EvidencePath)
            .Skip(1)
            .Where(row => row.Count >= 3)
            .ToDictionary(row => row[0], StringComparer.OrdinalIgnoreCase);

        foreach (EvidenceDefinition evidence in LoadAll<EvidenceDefinition>())
        {
            string number = evidence.name.Substring(0, Math.Min(3, evidence.name.Length));
            string id = number.Insert(1, "-");
            SetString(evidence, "id", id);
            if (rows.TryGetValue(id, out List<string> row))
            {
                SetString(evidence, "displayName", row[1]);
                SetString(evidence, "description", row[2]);
            }
            SetObject(evidence, "image", FindSprite($"EVD_evidence_{number.ToLowerInvariant()}"));
        }
    }

    private static void PopulateMaps()
    {
        foreach (MapDefinition map in LoadAll<MapDefinition>())
        {
            string token = map.name.Replace("MAP_", string.Empty);
            SetString(map, "id", map.name);
            SetObject(map, "baseLayer", FindSprite($"MAP_{token}_Base"));
            SetObject(map, "restrictedLayer", FindSprite($"MAP_{token}_Restricted"));
            SetObject(map, "technicalLayer", FindSprite($"MAP_{token}_Technical"));
        }
    }

    private static void PopulateDatabases()
    {
        ContentDatabase content = AssetDatabase.LoadAssetAtPath<ContentDatabase>(ContentRoot + "/Game/DATABASE_Content.asset");
        SetArray(content, "storyScenes", LoadAll<StorySceneDefinition>().Cast<UnityEngine.Object>().ToArray());
        SetArray(content, "locations", LoadAll<LocationDefinition>().Cast<UnityEngine.Object>().ToArray());
        SetArray(content, "evidence", LoadAll<EvidenceDefinition>().Cast<UnityEngine.Object>().ToArray());

        DialogueDatabase dialogue = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(ContentRoot + "/Game/DATABASE_Dialogue.asset");
        SetArray(dialogue, "sequences", LoadAll<DialogueSequence>().Cast<UnityEngine.Object>().ToArray());

        EvidenceDatabase evidence = AssetDatabase.LoadAssetAtPath<EvidenceDatabase>(ContentRoot + "/Game/DATABASE_Evidence.asset");
        SetArray(evidence, "evidence", LoadAll<EvidenceDefinition>().Cast<UnityEngine.Object>().ToArray());

        ConfigureCatalog("DATABASE_Audio", LoadAll<AudioCueProfile>());
        ConfigureCatalog("DATABASE_Characters", LoadAll<CharacterDefinition>());
        ConfigureCatalog("DATABASE_Locations", LoadAll<LocationDefinition>());
        ConfigureCatalog("DATABASE_Puzzles", LoadAll<PuzzleDefinition>());

        GameDefinition game = AssetDatabase.LoadAssetAtPath<GameDefinition>(ContentRoot + "/Game/GAME_UnderTheHorizon.asset");
        SetObject(game, "content", content);
    }

    private static void ConfigureCatalog<T>(string name, IEnumerable<T> entries)
        where T : UnityEngine.Object
    {
        ContentCatalog catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>($"{ContentRoot}/Game/{name}.asset");
        SetString(catalog, "id", name);
        SetArray(catalog, "entries", entries.Cast<UnityEngine.Object>().ToArray());
    }

    private static void BuildPrefabs()
    {
        Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        EnsureScreenPrefab("PF_SettingsScreen", typeof(SettingsScreen), ScreenId.Settings, panel);
        EnsureScreenPrefab("PF_CreditsScreen", typeof(CreditsScreen), ScreenId.Credits, panel);
        foreach (string absolutePath in Directory.GetFiles(Path.GetFullPath(PrefabRoot), "*.prefab", SearchOption.AllDirectories))
        {
            string path = ToAssetPath(absolutePath);
            string name = Path.GetFileNameWithoutExtension(path);
            if (name == "PF_AppBootstrap")
                SavePrefab(path, CreateAppBootstrap(name));
            else if (name == "PF_GameRoot")
                SavePrefab(path, CreateGameRootShell(name));
            else if (name == "PF_EventSystem")
                SavePrefab(path, new GameObject(name, typeof(EventSystem)));
            else if (name == "PF_CharacterView" || name == "PF_AmbientCharacterView")
                SavePrefab(path, CreateCharacterView(name));
            else if (TryGetScreenType(name, out Type screenType, out ScreenId id))
                SavePrefab(path, CreateScreen(name, screenType, id, panel));
            else
                SavePrefab(path, CreateVisualPrefab(name, panel));
        }
    }

    private static void BuildPresentationPrefabs()
    {
        Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        (string name, Type type, ScreenId id)[] screens =
        {
            ("PF_TitleScreen", typeof(TitleScreen), ScreenId.Title),
            ("PF_SaveSlotScreen", typeof(SaveSlotScreen), ScreenId.SaveSlot),
            ("PF_DialogueScreen", typeof(DialogueScreen), ScreenId.Dialogue),
        };

        foreach ((string name, Type type, ScreenId id) in screens)
        {
            string path = $"{PrefabRoot}/UI/{name}.prefab";
            SavePrefab(path, CreateScreen(name, type, id, panel));
        }
    }

    private static void EnsureScreenPrefab(
        string name,
        Type type,
        ScreenId id,
        Sprite panel)
    {
        string path = PrefabRoot + "/UI/" + name + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            SavePrefab(path, CreateScreen(name, type, id, panel));
    }

    private static GameObject CreateAppBootstrap(string name)
    {
        GameObject root = new(name);
        root.AddComponent<AppLifetime>();
        AppBootstrap bootstrap = root.AddComponent<AppBootstrap>();
        SetObject(
            bootstrap,
            "gameDefinition",
            AssetDatabase.LoadAssetAtPath<GameDefinition>(
                ContentRoot + "/Game/GAME_UnderTheHorizon.asset"));
        return root;
    }

    private static GameObject CreateGameRootShell(string name)
    {
        GameObject root = new(name);
        new GameObject("WorldCanvas").transform.SetParent(root.transform);
        new GameObject("UICanvas").transform.SetParent(root.transform);
        new GameObject("Directors").transform.SetParent(root.transform);
        return root;
    }

    private static GameObject CreateCharacterView(string name)
    {
        GameObject root = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CharacterView));
        Image image = root.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(680f, 980f);
        SetObject(root.GetComponent<CharacterView>(), "image", image);
        return root;
    }

    private static GameObject CreateScreen(string name, Type type, ScreenId id, Sprite panel)
    {
        GameObject root = CreateVisualPrefab(name, panel);
        ScreenBase screen = (ScreenBase)root.AddComponent(type);
        SetEnum(screen, "id", (int)id);
        if (screen is DialogueScreen dialogue)
            BuildDialogueScreen(root.transform, dialogue);
        else if (screen is TitleScreen title)
            BuildTitleScreen(root.transform, title);
        else if (screen is SaveSlotScreen saveSlot)
            BuildSaveSlotScreen(root.transform, saveSlot);
        else if (screen is MapScreen map)
            BuildMapScreen(root.transform, map);
        else if (screen is InvestigationRecordScreen record)
            BuildInvestigationRecordScreen(root.transform, record);
        else if (screen is EvidenceBoardScreen evidenceBoard)
            BuildEvidenceBoardScreen(root.transform, evidenceBoard);
        else if (screen is PuzzleScreen puzzle)
            BuildPuzzleScreen(root.transform, puzzle);
        else if (screen is ExplorationScreen)
        {
            Image image = root.GetComponent<Image>();
            image.color = Color.clear;
            image.sprite = null;
            image.raycastTarget = false;
        }
        else if (screen is EndingScreen)
            BuildEndingScreen(root.transform);
        else if (screen is SettingsScreen || screen is CreditsScreen)
            BuildSecondaryScreen(root.transform, screen);
        root.SetActive(false);
        return root;
    }

    private static void BuildInvestigationRecordScreen(
        Transform root, InvestigationRecordScreen screen)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        Image background = root.GetComponent<Image>();
        background.color = new Color(0.012f, 0.022f, 0.035f, 1f);
        Text title = CreateText("Record Title", root, font, 42, TextAnchor.MiddleLeft);
        title.text = "조사 기록";
        title.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        SetRect(title.rectTransform, new Vector2(0.055f, 0.88f), new Vector2(0.48f, 0.97f));

        Image listPanel = CreateLayer("Evidence List", root).gameObject.AddComponent<Image>();
        listPanel.color = new Color(0.025f, 0.045f, 0.065f, 0.96f);
        SetRect(listPanel.rectTransform, new Vector2(0.055f, 0.15f), new Vector2(0.58f, 0.86f));
        Image detailPanel = CreateLayer("Evidence Detail", root).gameObject.AddComponent<Image>();
        detailPanel.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        detailPanel.type = Image.Type.Sliced;
        detailPanel.color = new Color(0.055f, 0.07f, 0.085f, 0.98f);
        SetRect(detailPanel.rectTransform, new Vector2(0.60f, 0.15f), new Vector2(0.945f, 0.86f));

        const int capacity = 18;
        var buttons = new Button[capacity];
        var images = new Image[capacity];
        var labels = new Text[capacity];
        Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            ProjectRoot + "/Art/UI/Cards/UI_card_evidence.png");
        for (var index = 0; index < capacity; index++)
        {
            GameObject card = new($"EvidenceCard{index + 1}", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image), typeof(Button));
            card.transform.SetParent(listPanel.transform, false);
            Image cardImage = card.GetComponent<Image>();
            cardImage.sprite = cardSprite;
            cardImage.type = Image.Type.Sliced;
            cardImage.color = new Color(0.10f, 0.12f, 0.14f, 1f);
            Button button = card.GetComponent<Button>();
            button.targetGraphic = cardImage;
            int column = index % 3;
            int row = index / 3;
            float left = 0.025f + column * 0.325f;
            float top = 0.965f - row * 0.158f;
            SetRect((RectTransform)card.transform,
                new Vector2(left, top - 0.135f), new Vector2(left + 0.30f, top));
            Image thumbnail = CreateLayer("Thumbnail", card.transform).gameObject.AddComponent<Image>();
            thumbnail.preserveAspect = true;
            thumbnail.raycastTarget = false;
            SetRect(thumbnail.rectTransform, new Vector2(0.04f, 0.30f), new Vector2(0.34f, 0.92f));
            Text label = CreateText("Label", card.transform, font, 16, TextAnchor.MiddleLeft);
            label.color = new Color(0.92f, 0.88f, 0.78f, 1f);
            label.raycastTarget = false;
            SetRect(label.rectTransform, new Vector2(0.38f, 0.10f), new Vector2(0.96f, 0.90f));
            buttons[index] = button;
            images[index] = thumbnail;
            labels[index] = label;
        }

        Text empty = CreateText("Empty Label", listPanel.transform, font, 24, TextAnchor.MiddleCenter);
        empty.text = "아직 수집된 증거가 없습니다.";
        empty.color = new Color(0.72f, 0.69f, 0.62f, 1f);
        SetRect(empty.rectTransform, new Vector2(0.12f, 0.35f), new Vector2(0.88f, 0.65f));
        Image detailImage = CreateLayer("Detail Image", detailPanel.transform).gameObject.AddComponent<Image>();
        detailImage.preserveAspect = true;
        detailImage.raycastTarget = false;
        SetRect(detailImage.rectTransform, new Vector2(0.10f, 0.53f), new Vector2(0.90f, 0.92f));
        Text detailTitle = CreateText("Detail Title", detailPanel.transform, font, 28, TextAnchor.MiddleLeft);
        detailTitle.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        SetRect(detailTitle.rectTransform, new Vector2(0.10f, 0.40f), new Vector2(0.90f, 0.52f));
        Text detailBody = CreateText("Detail Body", detailPanel.transform, font, 20, TextAnchor.UpperLeft);
        detailBody.color = new Color(0.88f, 0.85f, 0.78f, 1f);
        detailBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        SetRect(detailBody.rectTransform, new Vector2(0.10f, 0.10f), new Vector2(0.90f, 0.39f));
        Button back = CreateTitleButton("BackButton", root, font, "돌아가기", false);
        SetRect((RectTransform)back.transform,
            new Vector2(0.055f, 0.055f), new Vector2(0.18f, 0.12f));

        SetArray(screen, "cardButtons", buttons.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "cardImages", images.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "cardLabels", labels.Cast<UnityEngine.Object>().ToArray());
        SetObject(screen, "detailImage", detailImage);
        SetObject(screen, "detailTitle", detailTitle);
        SetObject(screen, "detailBody", detailBody);
        SetObject(screen, "emptyLabel", empty);
        SetObject(screen, "backButton", back);
        Button board = CreateTitleButton("EvidenceBoardButton", root, font, "증거 보드", true);
        SetRect((RectTransform)board.transform, new Vector2(0.20f, 0.055f), new Vector2(0.36f, 0.12f));
        SetObject(screen, "boardButton", board);
        Button filter = CreateTitleButton("EvidenceFilterButton", root, font, "전체 증거", false);
        SetRect((RectTransform)filter.transform, new Vector2(0.38f, 0.055f), new Vector2(0.54f, 0.12f));
        SetObject(screen, "filterButton", filter);
        SetObject(screen, "filterLabel", filter.GetComponentInChildren<Text>());
    }

    private static void BuildEvidenceBoardScreen(Transform root, EvidenceBoardScreen screen)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        root.GetComponent<Image>().color = new Color(0.008f, 0.015f, 0.025f, 1f);
        Text title = CreateText("Board Title", root, font, 42, TextAnchor.MiddleLeft);
        title.text = "증거 보드"; title.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        SetRect(title.rectTransform, new Vector2(0.045f, 0.89f), new Vector2(0.42f, 0.97f));
        Text progress = CreateText("Progress", root, font, 19, TextAnchor.MiddleRight);
        progress.color = new Color(0.78f, 0.74f, 0.66f, 1f);
        SetRect(progress.rectTransform, new Vector2(0.55f, 0.90f), new Vector2(0.95f, 0.96f));
        Image evidencePanel = CreateLayer("Evidence Nodes", root).gameObject.AddComponent<Image>();
        evidencePanel.color = new Color(0.022f, 0.038f, 0.055f, 0.97f);
        SetRect(evidencePanel.rectTransform, new Vector2(0.045f, 0.16f), new Vector2(0.52f, 0.87f));
        var evidenceButtons = new Button[18]; var evidenceImages = new Image[18]; var evidenceLabels = new Text[18];
        for (var index = 0; index < 18; index++)
        {
            Button button = CreateTitleButton($"EvidenceNode{index + 1}", evidencePanel.transform, font, string.Empty, false);
            int column = index % 3; int row = index / 3;
            float left = 0.025f + column * 0.325f; float top = 0.97f - row * 0.158f;
            SetRect((RectTransform)button.transform, new Vector2(left, top - 0.135f), new Vector2(left + 0.30f, top));
            Image thumbnail = CreateLayer("Image", button.transform).gameObject.AddComponent<Image>();
            thumbnail.preserveAspect = true; thumbnail.raycastTarget = false;
            SetRect(thumbnail.rectTransform, new Vector2(0.05f, 0.30f), new Vector2(0.35f, 0.90f));
            Text label = button.GetComponentInChildren<Text>(); label.alignment = TextAnchor.MiddleLeft;
            SetRect(label.rectTransform, new Vector2(0.38f, 0.08f), new Vector2(0.96f, 0.92f));
            evidenceButtons[index] = button; evidenceImages[index] = thumbnail; evidenceLabels[index] = label;
        }
        Text empty = CreateText("Empty Label", evidencePanel.transform, font, 22, TextAnchor.MiddleCenter);
        empty.text = "연결할 증거가 없습니다."; empty.color = new Color(0.7f, 0.67f, 0.61f, 1f);
        SetRect(empty.rectTransform, new Vector2(0.15f, 0.4f), new Vector2(0.85f, 0.6f));
        Image theoryPanel = CreateLayer("Theory Slots", root).gameObject.AddComponent<Image>();
        theoryPanel.color = new Color(0.035f, 0.025f, 0.045f, 0.98f);
        SetRect(theoryPanel.rectTransform, new Vector2(0.54f, 0.38f), new Vector2(0.955f, 0.87f));
        var theoryButtons = new Button[6]; var theoryLabels = new Text[6];
        for (var index = 0; index < 6; index++)
        {
            Button button = CreateTitleButton($"TheorySlot{index + 1}", theoryPanel.transform, font, string.Empty, false);
            int column = index % 2; int row = index / 2;
            float left = 0.035f + column * 0.49f; float top = 0.95f - row * 0.30f;
            SetRect((RectTransform)button.transform, new Vector2(left, top - 0.24f), new Vector2(left + 0.45f, top));
            Text label = button.GetComponentInChildren<Text>(); label.fontSize = 18;
            theoryButtons[index] = button; theoryLabels[index] = label;
        }
        Image detail = CreateLayer("Connection Detail", root).gameObject.AddComponent<Image>();
        detail.color = new Color(0.055f, 0.06f, 0.075f, 0.98f);
        SetRect(detail.rectTransform, new Vector2(0.54f, 0.16f), new Vector2(0.955f, 0.36f));
        Text detailTitle = CreateText("Detail Title", detail.transform, font, 24, TextAnchor.MiddleLeft);
        detailTitle.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        SetRect(detailTitle.rectTransform, new Vector2(0.05f, 0.62f), new Vector2(0.95f, 0.94f));
        Text detailBody = CreateText("Detail Body", detail.transform, font, 16, TextAnchor.UpperLeft);
        detailBody.color = new Color(0.86f, 0.83f, 0.77f, 1f); detailBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        SetRect(detailBody.rectTransform, new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.62f));
        Button back = CreateTitleButton("BackButton", root, font, "조사 기록", false);
        SetRect((RectTransform)back.transform, new Vector2(0.045f, 0.055f), new Vector2(0.18f, 0.12f));
        SetArray(screen, "evidenceButtons", evidenceButtons.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "evidenceImages", evidenceImages.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "evidenceLabels", evidenceLabels.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "theoryButtons", theoryButtons.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "theoryLabels", theoryLabels.Cast<UnityEngine.Object>().ToArray());
        SetObject(screen, "detailTitle", detailTitle); SetObject(screen, "detailBody", detailBody);
        SetObject(screen, "progressLabel", progress); SetObject(screen, "emptyLabel", empty); SetObject(screen, "backButton", back);
    }

    private static void BuildPuzzleScreen(Transform root, PuzzleScreen screen)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        root.GetComponent<Image>().color = new Color(0.008f, 0.014f, 0.022f, 0.98f);
        Image frame = CreateLayer("Puzzle Frame", root).gameObject.AddComponent<Image>();
        frame.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        frame.type = Image.Type.Sliced; frame.color = new Color(0.06f, 0.075f, 0.09f, 0.98f);
        SetRect(frame.rectTransform, new Vector2(0.10f, 0.09f), new Vector2(0.90f, 0.91f));
        Text title = CreateText("Puzzle Title", frame.transform, font, 38, TextAnchor.MiddleCenter);
        title.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        SetRect(title.rectTransform, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.96f));
        Text instruction = CreateText("Instruction", frame.transform, font, 20, TextAnchor.MiddleCenter);
        instruction.color = new Color(0.85f, 0.82f, 0.75f, 1f);
        SetRect(instruction.rectTransform, new Vector2(0.10f, 0.73f), new Vector2(0.90f, 0.84f));
        Image workspace = CreateLayer("Controller Workspace", frame.transform).gameObject.AddComponent<Image>();
        workspace.color = new Color(0.015f, 0.025f, 0.038f, 1f);
        SetRect(workspace.rectTransform, new Vector2(0.08f, 0.27f), new Vector2(0.92f, 0.72f));
        Text hint = CreateText("Hint", workspace.transform, font, 21, TextAnchor.MiddleCenter);
        hint.color = new Color(0.94f, 0.78f, 0.43f, 1f);
        SetRect(hint.rectTransform, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.88f));
        Text result = CreateText("Result", frame.transform, font, 26, TextAnchor.MiddleCenter);
        result.color = new Color(0.94f, 0.78f, 0.43f, 1f);
        SetRect(result.rectTransform, new Vector2(0.18f, 0.15f), new Vector2(0.82f, 0.25f));
        Button cancel = CreateTitleButton("CancelButton", frame.transform, font, "나가기", false);
        Button hintButton = CreateTitleButton("HintButton", frame.transform, font, "힌트", true);
        Button returnButton = CreateTitleButton("ReturnButton", frame.transform, font, "조사로 돌아가기", true);
        SetRect((RectTransform)cancel.transform, new Vector2(0.08f, 0.07f), new Vector2(0.25f, 0.15f));
        SetRect((RectTransform)hintButton.transform, new Vector2(0.75f, 0.07f), new Vector2(0.92f, 0.15f));
        SetRect((RectTransform)returnButton.transform, new Vector2(0.37f, 0.07f), new Vector2(0.63f, 0.15f));
        SetObject(screen, "titleLabel", title); SetObject(screen, "instructionLabel", instruction);
        SetObject(screen, "hintLabel", hint); SetObject(screen, "resultLabel", result);
        SetObject(screen, "hintButton", hintButton); SetObject(screen, "cancelButton", cancel);
        SetObject(screen, "returnButton", returnButton);
    }

    private static void BuildMapScreen(Transform root, MapScreen screen)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        Image background = root.GetComponent<Image>();
        background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            ProjectRoot + "/Art/UI/Map/MAP_ui_map_screen_backdrop.png");
        background.color = Color.white;

        Text title = CreateText("Map Title", root, font, 42, TextAnchor.MiddleLeft);
        title.text = "M.V. ELYSIUM · DECK PLAN";
        title.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        SetRect(title.rectTransform, new Vector2(0.055f, 0.88f), new Vector2(0.60f, 0.97f));
        Text location = CreateText("Current Location", root, font, 22, TextAnchor.MiddleRight);
        location.color = new Color(0.80f, 0.76f, 0.67f, 1f);
        SetRect(location.rectTransform, new Vector2(0.60f, 0.89f), new Vector2(0.94f, 0.96f));

        Image viewport = CreateLayer("Map Viewport", root).gameObject.AddComponent<Image>();
        viewport.color = new Color(0.015f, 0.025f, 0.035f, 0.95f);
        SetRect(viewport.rectTransform, new Vector2(0.19f, 0.15f), new Vector2(0.94f, 0.86f));
        Image baseLayer = CreateMapLayer("Base Layer", viewport.transform);
        Image restrictedLayer = CreateMapLayer("Restricted Layer", viewport.transform);
        Image technicalLayer = CreateMapLayer("Technical Layer", viewport.transform);

        MapDefinition[] maps = LoadAll<MapDefinition>()
            .OrderBy(map => map.Id, StringComparer.Ordinal).ToArray();
        var deckButtons = new Button[maps.Length];
        for (var index = 0; index < maps.Length; index++)
        {
            string label = maps[index].Id.Replace("MAP_", string.Empty);
            deckButtons[index] = CreateTitleButton(
                $"{label}Button", root, font, label, index == 0);
            float top = 0.83f - index * 0.105f;
            SetRect((RectTransform)deckButtons[index].transform,
                new Vector2(0.055f, top - 0.075f), new Vector2(0.17f, top));
        }

        Toggle restricted = CreateSettingsToggle(root, font, "제한 구역", 0.18f);
        SetRect((RectTransform)restricted.transform,
            new Vector2(0.21f, 0.07f), new Vector2(0.39f, 0.13f));
        Toggle technical = CreateSettingsToggle(root, font, "기술 도면", 0.18f);
        SetRect((RectTransform)technical.transform,
            new Vector2(0.41f, 0.07f), new Vector2(0.59f, 0.13f));
        Button back = CreateTitleButton("BackButton", root, font, "돌아가기", false);
        SetRect((RectTransform)back.transform,
            new Vector2(0.055f, 0.055f), new Vector2(0.17f, 0.12f));
        Text deckLabel = CreateText("Deck Label", viewport.transform, font, 24, TextAnchor.UpperLeft);
        deckLabel.color = new Color(0.965f, 0.827f, 0.529f, 0.9f);
        SetRect(deckLabel.rectTransform, new Vector2(0.03f, 0.88f), new Vector2(0.35f, 0.97f));

        SetObject(screen, "baseLayer", baseLayer);
        SetObject(screen, "restrictedLayer", restrictedLayer);
        SetObject(screen, "technicalLayer", technicalLayer);
        SetObject(screen, "deckLabel", deckLabel);
        SetObject(screen, "locationLabel", location);
        SetObject(screen, "restrictedToggle", restricted);
        SetObject(screen, "technicalToggle", technical);
        SetObject(screen, "backButton", back);
        SetArray(screen, "maps", maps.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "deckButtons", deckButtons.Cast<UnityEngine.Object>().ToArray());
    }

    private static Image CreateMapLayer(string name, Transform parent)
    {
        Image image = CreateLayer(name, parent).gameObject.AddComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        SetRect(image.rectTransform, new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.95f));
        return image;
    }

    private static void BuildSecondaryScreen(Transform root, ScreenBase screen)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        bool credits = screen is CreditsScreen;
        Text title = CreateText("Title", root, font, 48, TextAnchor.MiddleCenter);
        title.text = credits ? "크레딧" : "설정";
        title.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        SetRect(title.rectTransform, new Vector2(0.2f, 0.73f), new Vector2(0.8f, 0.85f));
        Text body = CreateText("Body", root, font, 28, TextAnchor.UpperCenter);
        body.text = credits
            ? "UNDER THE HORIZON\n기획 · 아트 · 개발 크레딧"
            : string.Empty;
        body.color = new Color(0.965f, 0.941f, 0.867f, 1f);
        SetRect(body.rectTransform, new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.68f));
        if (screen is SettingsScreen settings)
        {
            Slider master = CreateSettingsSlider(root, font, "전체 음량", 0.68f);
            Slider music = CreateSettingsSlider(root, font, "배경 음악", 0.60f);
            Slider sfx = CreateSettingsSlider(root, font, "효과음", 0.52f);
            Dropdown resolution = CreateSettingsDropdown(root, font, "해상도", 0.42f);
            Toggle fullscreen = CreateSettingsToggle(root, font, "전체 화면", 0.34f);
            Toggle reducedMotion = CreateSettingsToggle(root, font, "움직임 줄이기", 0.26f);
            Dropdown textSpeed = CreateSettingsDropdown(root, font, "대화 표시 속도", 0.18f);
            Button apply = CreateTitleButton(
                "ApplyDisplayButton", root, font, "화면 설정 적용", true);
            SetRect((RectTransform)apply.transform,
                new Vector2(0.44f, 0.08f), new Vector2(0.72f, 0.14f));
            SetObject(settings, "masterSlider", master);
            SetObject(settings, "musicSlider", music);
            SetObject(settings, "sfxSlider", sfx);
            SetObject(settings, "resolutionDropdown", resolution);
            SetObject(settings, "fullscreenToggle", fullscreen);
            SetObject(settings, "applyDisplayButton", apply);
            SetObject(settings, "reducedMotionToggle", reducedMotion);
            SetObject(settings, "textSpeedDropdown", textSpeed);
        }
        Button back = CreateTitleButton("BackButton", root, font, "뒤로", false);
        SetRect((RectTransform)back.transform, new Vector2(0.05f, 0.06f), new Vector2(0.20f, 0.12f));
        SetObject(screen, "backButton", back);
    }

    private static Dropdown CreateSettingsDropdown(
        Transform root, Font font, string label, float centerY)
    {
        Text caption = CreateText(label + " Label", root, font, 26, TextAnchor.MiddleLeft);
        caption.text = label;
        caption.color = new Color(0.965f, 0.941f, 0.867f, 1f);
        SetRect(caption.rectTransform,
            new Vector2(0.28f, centerY - 0.025f),
            new Vector2(0.43f, centerY + 0.025f));

        GameObject dropdownObject = new(label + " Dropdown",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Dropdown));
        dropdownObject.transform.SetParent(root, false);
        SetRect((RectTransform)dropdownObject.transform,
            new Vector2(0.44f, centerY - 0.035f),
            new Vector2(0.72f, centerY + 0.035f));
        Image background = dropdownObject.GetComponent<Image>();
        background.color = new Color(0.035f, 0.07f, 0.10f, 0.98f);

        Text selected = CreateText("Label", dropdownObject.transform, font, 22, TextAnchor.MiddleLeft);
        selected.color = new Color(0.965f, 0.941f, 0.867f, 1f);
        SetRect(selected.rectTransform, new Vector2(0.06f, 0f), new Vector2(0.88f, 1f));
        Text arrow = CreateText("Arrow", dropdownObject.transform, font, 22, TextAnchor.MiddleCenter);
        arrow.text = "▼";
        SetRect(arrow.rectTransform, new Vector2(0.88f, 0f), Vector2.one);

        GameObject template = new("Template", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
        template.transform.SetParent(dropdownObject.transform, false);
        template.SetActive(false);
        RectTransform templateRect = (RectTransform)template.transform;
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.anchoredPosition = new Vector2(0f, -4f);
        templateRect.sizeDelta = new Vector2(0f, 240f);
        template.GetComponent<Image>().color = new Color(0.018f, 0.035f, 0.052f, 1f);
        GameObject item = new("Item", typeof(RectTransform), typeof(Toggle));
        item.transform.SetParent(template.transform, false);
        SetRect((RectTransform)item.transform, Vector2.zero, Vector2.one);
        Text itemLabel = CreateText("Item Label", item.transform, font, 21, TextAnchor.MiddleLeft);
        SetRect(itemLabel.rectTransform, new Vector2(0.06f, 0f), Vector2.one);

        Dropdown dropdown = dropdownObject.GetComponent<Dropdown>();
        dropdown.targetGraphic = background;
        dropdown.captionText = selected;
        dropdown.template = templateRect;
        dropdown.itemText = itemLabel;
        return dropdown;
    }

    private static Toggle CreateSettingsToggle(
        Transform root, Font font, string label, float centerY)
    {
        GameObject toggleObject = new(label + " Toggle", typeof(RectTransform), typeof(Toggle));
        toggleObject.transform.SetParent(root, false);
        SetRect((RectTransform)toggleObject.transform,
            new Vector2(0.44f, centerY - 0.03f),
            new Vector2(0.72f, centerY + 0.03f));
        Image box = CreateLayer("Background", toggleObject.transform).gameObject.AddComponent<Image>();
        box.color = new Color(0.035f, 0.07f, 0.10f, 1f);
        box.rectTransform.anchorMin = new Vector2(0f, 0.15f);
        box.rectTransform.anchorMax = new Vector2(0.12f, 0.85f);
        Text mark = CreateText("Checkmark", box.transform, font, 26, TextAnchor.MiddleCenter);
        mark.text = "✓";
        mark.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        SetRect(mark.rectTransform, Vector2.zero, Vector2.one);
        Text toggleLabel = CreateText("Label", toggleObject.transform, font, 24, TextAnchor.MiddleLeft);
        toggleLabel.text = label;
        SetRect(toggleLabel.rectTransform, new Vector2(0.16f, 0f), Vector2.one);
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.targetGraphic = box;
        toggle.graphic = mark;
        toggle.isOn = true;
        return toggle;
    }

    private static Slider CreateSettingsSlider(
        Transform root,
        Font font,
        string label,
        float centerY)
    {
        Text caption = CreateText(label + " Label", root, font, 26, TextAnchor.MiddleLeft);
        caption.text = label;
        caption.color = new Color(0.965f, 0.941f, 0.867f, 1f);
        SetRect(caption.rectTransform, new Vector2(0.28f, centerY - 0.025f), new Vector2(0.43f, centerY + 0.025f));
        GameObject sliderObject = new(label + " Slider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(root, false);
        SetRect(
            (RectTransform)sliderObject.transform,
            new Vector2(0.44f, centerY - 0.02f),
            new Vector2(0.72f, centerY + 0.02f)
        );
        Image track = CreateLayer("Track", sliderObject.transform).gameObject.AddComponent<Image>();
        track.color = new Color(0.055f, 0.094f, 0.133f, 1f);
        Image fill = CreateLayer("Fill", track.transform).gameObject.AddComponent<Image>();
        fill.color = new Color(0.718f, 0.522f, 0.212f, 1f);
        RectTransform handle = CreateLayer("Handle", sliderObject.transform);
        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        handle.anchorMin = handle.anchorMax = new Vector2(1f, 0.5f);
        handle.sizeDelta = new Vector2(24f, 44f);
        Slider slider = sliderObject.GetComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.value = 1f;
        return slider;
    }

    private static void BuildDialogueScreen(Transform root, DialogueScreen screen)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-Regular.ttf");
        Image rootImage = root.GetComponent<Image>();
        rootImage.sprite = null;
        rootImage.color = Color.clear;
        rootImage.raycastTarget = false;

        GameObject dimObject = new("Dialogue Focus Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dimObject.transform.SetParent(root, false);
        Image dim = dimObject.GetComponent<Image>();
        dim.color = new Color(0.005f, 0.008f, 0.025f, 0.16f);
        dim.raycastTarget = false;
        SetRect(dimObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        GameObject portraitObject = new("Speaker Portrait", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        portraitObject.transform.SetParent(root, false);
        Image portrait = portraitObject.GetComponent<Image>();
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        SetRect(portrait.rectTransform, new Vector2(0.035f, 0.32f), new Vector2(0.31f, 0.94f));
        portraitObject.SetActive(false);

        GameObject panelObject = new("Dialogue Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(root, false);
        Image panel = panelObject.GetComponent<Image>();
        panel.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ProjectRoot + "/Art/UI/Panels/UI_panel_dialogue.png");
        panel.color = Color.white;
        panel.type = Image.Type.Sliced;
        SetRect(panelObject.GetComponent<RectTransform>(), new Vector2(0.055f, 0.055f), new Vector2(0.945f, 0.30f));

        Text scene = CreateText("SceneLabel", panelObject.transform, font, 18, TextAnchor.MiddleLeft);
        scene.color = new Color(0.79f, 0.60f, 0.29f, 0.78f);
        SetRect(scene.rectTransform, new Vector2(0.07f, 0.84f), new Vector2(0.93f, 0.94f));

        GameObject nameplateObject = new("Speaker Nameplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        nameplateObject.transform.SetParent(root, false);
        Image nameplate = nameplateObject.GetComponent<Image>();
        nameplate.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ProjectRoot + "/Art/UI/Panels/UI_label_nameplate.png");
        nameplate.type = Image.Type.Sliced;
        SetRect(nameplateObject.GetComponent<RectTransform>(), new Vector2(0.075f, 0.285f), new Vector2(0.28f, 0.345f));
        Text speaker = CreateText("SpeakerLabel", nameplateObject.transform, font, 28, TextAnchor.MiddleCenter);
        speaker.color = new Color(0.025f, 0.043f, 0.071f);
        SetRect(speaker.rectTransform, Vector2.zero, Vector2.one);

        Text body = CreateText("BodyLabel", panelObject.transform, font, 30, TextAnchor.UpperLeft);
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        body.verticalOverflow = VerticalWrapMode.Overflow;
        body.color = new Color(0.025f, 0.043f, 0.071f);
        SetRect(body.rectTransform, new Vector2(0.07f, 0.18f), new Vector2(0.91f, 0.78f));

        Button advance = CreateSpriteButton(
            "AdvanceButton", root, font, string.Empty,
            ProjectRoot + "/Art/UI/Dialogue/UI_btn_dialogue_advance_normal.png",
            ProjectRoot + "/Art/UI/Dialogue/UI_btn_dialogue_advance_pressed.png",
            out Text advanceText);
        SetRect((RectTransform)advance.transform, new Vector2(0.875f, 0.075f), new Vector2(0.925f, 0.145f));

        Button[] choices = new Button[3];
        Text[] choiceLabels = new Text[3];
        for (var i = 0; i < choices.Length; i++)
        {
            choices[i] = CreateSpriteButton(
                $"Choice{i + 1}", root, font, string.Empty,
                ProjectRoot + "/Art/UI/Buttons/UI_btn_choice_normal.png",
                ProjectRoot + "/Art/UI/Buttons/UI_btn_choice_pressed.png",
                out choiceLabels[i]);
            choices[i].gameObject.AddComponent<DialogueChoiceBinding>();
            choices[i].gameObject.AddComponent<CanvasGroup>();
            var top = 0.29f - (i * 0.07f);
            SetRect((RectTransform)choices[i].transform, new Vector2(0.10f, top - 0.058f), new Vector2(0.90f, top));
            choices[i].gameObject.SetActive(false);
        }

        SetObject(screen, "sceneLabel", scene);
        SetObject(screen, "speakerLabel", speaker);
        SetObject(screen, "bodyLabel", body);
        SetObject(screen, "portraitImage", portrait);
        SetObject(screen, "portraitGroup", portraitObject.GetComponent<CanvasGroup>());
        SetObject(screen, "advanceButton", advance);
        SetObject(screen, "advanceLabel", advanceText);
        SetArray(screen, "choiceButtons", choices.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "choiceLabels", choiceLabels.Cast<UnityEngine.Object>().ToArray());
        SetObject(screen, "typewriterClip", FindAudio("Typewriter"));
        SetFloat(screen, "charactersPerSecond", 45f);
    }

    private static void BuildTitleScreen(Transform root, TitleScreen screen)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        Image background = root.GetComponent<Image>();
        background.sprite = null;
        background.color = Color.clear;
        background.raycastTarget = false;

        Sprite titleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            ProjectRoot + "/Art/UI/Screens/UI_title_background.png"
        );
        Image artwork = CreateLayer("Title Background", root).gameObject.AddComponent<Image>();
        artwork.sprite = titleSprite;
        artwork.color = Color.white;
        artwork.raycastTarget = false;
        AspectRatioFitter artworkAspect = artwork.gameObject.AddComponent<AspectRatioFitter>();
        artworkAspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        artworkAspect.aspectRatio = titleSprite.rect.width / titleSprite.rect.height;
        artwork.transform.SetAsFirstSibling();

        GameObject shadeObject = new("Left Readability Shade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        shadeObject.transform.SetParent(root, false);
        Image shade = shadeObject.GetComponent<Image>();
        shade.color = new Color(0.005f, 0.008f, 0.025f, 0.34f);
        shade.raycastTarget = false;
        SetRect(shadeObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        GameObject logoObject = new("Under the Horizon Logo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        logoObject.transform.SetParent(root, false);
        Image logo = logoObject.GetComponent<Image>();
        logo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ProjectRoot + "/Art/Branding/UI_logo_transparent.png");
        logo.preserveAspect = true;
        logo.raycastTarget = false;
        SetRect(logoObject.GetComponent<RectTransform>(), new Vector2(0.025f, 0.28f), new Vector2(0.48f, 0.96f));

        Button start = CreateTitleButton("StartButton", root, font, "시작", true);
        SetRect((RectTransform)start.transform, new Vector2(0.04f, 0.20f), new Vector2(0.30f, 0.255f));
        string[] secondaryLabels = { "설정", "크레딧", "종료" };
        Button[] secondaryButtons = new Button[secondaryLabels.Length];
        for (var i = 0; i < secondaryLabels.Length; i++)
        {
            Button secondary = CreateTitleButton(
                secondaryLabels[i] + " Button",
                root,
                font,
                secondaryLabels[i],
                false
            );
            secondaryButtons[i] = secondary;
            float top = 0.195f - i * 0.05f;
            SetRect(
                (RectTransform)secondary.transform,
                new Vector2(0.04f, top - 0.042f),
                new Vector2(0.30f, top)
            );
        }
        SetObject(screen, "startButton", start);
        SetObject(screen, "settingsButton", secondaryButtons[0]);
        SetObject(screen, "creditsButton", secondaryButtons[1]);
        SetObject(screen, "quitButton", secondaryButtons[2]);
        SetObject(screen, "ambientParticles",
            AssetDatabase.LoadAssetAtPath<AmbientParticleProfile>(
                ContentRoot + "/UI/UI_AMBIENCE_TITLE.asset"));
    }

    private static Button CreateTitleButton(
        string name,
        Transform parent,
        Font font,
        string label,
        bool primary)
    {
        Button button = CreateButton(name, parent, font, label, out Text text);
        button.transition = Selectable.Transition.ColorTint;
        Image background = button.GetComponent<Image>();
        background.color = primary
            ? new Color(0.19f, 0.10f, 0.015f, 0.96f)
            : new Color(0.055f, 0.094f, 0.133f, 0.94f);
        Outline outline = button.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.718f, 0.522f, 0.212f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);
        text.fontSize = 26;
        text.color = new Color(0.965f, 0.941f, 0.867f, 1f);
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        return button;
    }

    private static void BuildEndingScreen(Transform root)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        Text title = CreateText("EndingTitle", root, font, 54, TextAnchor.MiddleCenter);
        title.text = "UNDER THE HORIZON";
        title.color = new Color(0.88f, 0.72f, 0.35f);
        SetRect(title.rectTransform, new Vector2(0.15f, 0.48f), new Vector2(0.85f, 0.62f));
        Text message = CreateText("EndingMessage", root, font, 28, TextAnchor.MiddleCenter);
        message.text = "현재 구현된 스토리를 모두 진행했습니다.";
        SetRect(message.rectTransform, new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.48f));
    }

    private static void BuildSaveSlotScreen(Transform root, SaveSlotScreen screen)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        Image background = root.GetComponent<Image>();
        background.color = new Color(0.012f, 0.022f, 0.035f, 1f);

        Image glow = CreateLayer("Save Screen Glow", root).gameObject.AddComponent<Image>();
        glow.color = new Color(0.10f, 0.18f, 0.23f, 0.35f);
        glow.raycastTarget = false;
        SetRect(glow.rectTransform, new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.88f));

        Image frame = CreateLayer("Save Slot Frame", root).gameObject.AddComponent<Image>();
        frame.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        frame.type = Image.Type.Sliced;
        frame.color = new Color(0.62f, 0.46f, 0.22f, 0.92f);
        frame.raycastTarget = false;
        SetRect(frame.rectTransform, new Vector2(0.055f, 0.17f), new Vector2(0.945f, 0.78f));

        Text title = CreateText("Save Slot Title", root, font, 42, TextAnchor.MiddleCenter);
        title.text = "저장 슬롯 선택";
        title.color = new Color(0.88f, 0.72f, 0.35f);
        SetRect(title.rectTransform, new Vector2(0.25f, 0.76f), new Vector2(0.75f, 0.87f));

        Text subtitle = CreateText("Save Slot Subtitle", root, font, 20, TextAnchor.MiddleCenter);
        subtitle.text = "계속할 항해 기록을 선택하세요";
        subtitle.color = new Color(0.78f, 0.76f, 0.69f, 1f);
        SetRect(subtitle.rectTransform, new Vector2(0.25f, 0.70f), new Vector2(0.75f, 0.76f));

        var buttons = new Button[3];
        var chapterLabels = new Text[3];
        var statusLabels = new Text[3];
        var actionLabels = new Text[3];
        var deleteButtons = new Button[3];
        for (var index = 0; index < buttons.Length; index++)
        {
            buttons[index] = CreateTitleButton(
                $"Slot{index + 1}Button",
                root,
                font,
                $"항해 기록 {index + 1}",
                index == 0
            );
            Image slotImage = buttons[index].GetComponent<Image>();
            slotImage.color = index == 0
                ? new Color(0.20f, 0.12f, 0.035f, 0.98f)
                : new Color(0.035f, 0.07f, 0.10f, 0.96f);
            Text slotLabel = buttons[index].GetComponentInChildren<Text>();
            slotLabel.fontSize = 28;
            slotLabel.color = new Color(0.965f, 0.941f, 0.867f, 1f);
            slotLabel.alignment = TextAnchor.UpperCenter;
            SetRect(slotLabel.rectTransform,
                new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.94f));
            chapterLabels[index] = CreateText(
                "Chapter", buttons[index].transform, font, 28, TextAnchor.MiddleCenter);
            chapterLabels[index].color = new Color(0.80f, 0.61f, 0.30f, 1f);
            SetRect(chapterLabels[index].rectTransform,
                new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.68f));
            statusLabels[index] = CreateText(
                "Status", buttons[index].transform, font, 20, TextAnchor.MiddleCenter);
            statusLabels[index].color = new Color(0.74f, 0.72f, 0.66f, 1f);
            SetRect(statusLabels[index].rectTransform,
                new Vector2(0.08f, 0.29f), new Vector2(0.92f, 0.48f));
            actionLabels[index] = CreateText(
                "Action", buttons[index].transform, font, 22, TextAnchor.MiddleCenter);
            actionLabels[index].color = new Color(0.965f, 0.827f, 0.529f, 1f);
            SetRect(actionLabels[index].rectTransform,
                new Vector2(0.08f, 0.07f), new Vector2(0.92f, 0.24f));
            float left = 0.075f + index * 0.2925f;
            SetRect(
                (RectTransform)buttons[index].transform,
                new Vector2(left, 0.27f),
                new Vector2(left + 0.275f, 0.68f)
            );
            deleteButtons[index] = CreateTitleButton(
                $"DeleteSlot{index + 1}Button", root, font, "삭제", false);
            deleteButtons[index].GetComponent<Image>().color =
                new Color(0.16f, 0.035f, 0.045f, 0.96f);
            SetRect((RectTransform)deleteButtons[index].transform,
                new Vector2(left, 0.205f), new Vector2(left + 0.275f, 0.255f));
        }
        SetArray(screen, "slotButtons", buttons.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "chapterLabels", chapterLabels.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "statusLabels", statusLabels.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "actionLabels", actionLabels.Cast<UnityEngine.Object>().ToArray());
        SetArray(screen, "deleteButtons", deleteButtons.Cast<UnityEngine.Object>().ToArray());
    }

    private static Text CreateText(
        string name, Transform parent, Font font, int size, TextAnchor alignment)
    {
        GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        Text text = gameObject.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static Button CreateButton(
        string name, Transform parent, Font font, string label, out Text text)
    {
        GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        Image image = gameObject.GetComponent<Image>();
        image.color = new Color(0.08f, 0.12f, 0.20f, 0.96f);
        Button button = gameObject.GetComponent<Button>();
        text = CreateText("Label", gameObject.transform, font, 24, TextAnchor.MiddleCenter);
        text.text = label;
        SetRect(text.rectTransform, Vector2.zero, Vector2.one);
        return button;
    }

    private static Button CreateSpriteButton(
        string name, Transform parent, Font font, string label,
        string normalPath, string pressedPath, out Text text)
    {
        Button button = CreateButton(name, parent, font, label, out text);
        Image image = button.GetComponent<Image>();
        Sprite normal = AssetDatabase.LoadAssetAtPath<Sprite>(normalPath);
        Sprite pressed = AssetDatabase.LoadAssetAtPath<Sprite>(pressedPath);
        image.sprite = normal;
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        SpriteState state = button.spriteState;
        state.pressedSprite = pressed;
        state.selectedSprite = pressed;
        button.spriteState = state;
        text.color = new Color(0.025f, 0.043f, 0.071f);
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static GameObject CreateVisualPrefab(string name, Sprite panel)
    {
        GameObject root = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Image image = root.GetComponent<Image>();
        image.sprite = panel;
        image.color = panel == null ? new Color(0.05f, 0.07f, 0.13f, 0.94f) : Color.white;
        image.type = panel == null ? Image.Type.Simple : Image.Type.Sliced;
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return root;
    }

    private static void BuildScenes()
    {
        BuildBootstrapScene();
        BuildGameScene();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ProjectRoot + "/Scenes/Bootstrap.unity", true),
            new EditorBuildSettingsScene(ProjectRoot + "/Scenes/Game.unity", true),
        };
    }

    private static void BuildBootstrapScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject bootstrap = CreateAppBootstrap("AppBootstrap");
        GameObject canvasObject = new("LoadingOverlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        GameObject panel = CreateVisualPrefab("LoadingBackground", AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath));
        panel.transform.SetParent(canvasObject.transform, false);
        EditorSceneManager.SaveScene(scene, ProjectRoot + "/Scenes/Bootstrap.unity");
    }

    private static void BuildGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new("GameRoot");
        GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(root.transform);
        Camera gameCamera = cameraObject.GetComponent<Camera>();
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = new Color(0.005f, 0.008f, 0.015f, 1f);
        GameStateStore state = new GameObject("GameStateStore").AddComponent<GameStateStore>();
        state.transform.SetParent(root.transform);
        EvidenceDirector evidenceDirector = new GameObject("EvidenceDirector")
            .AddComponent<EvidenceDirector>();
        evidenceDirector.transform.SetParent(root.transform);
        SetObject(evidenceDirector, "state", state);
        SetObject(evidenceDirector, "database",
            AssetDatabase.LoadAssetAtPath<EvidenceDatabase>(
                ContentRoot + "/Game/DATABASE_Evidence.asset"));
        EvidenceBoardDirector evidenceBoardDirector = new GameObject("EvidenceBoardDirector")
            .AddComponent<EvidenceBoardDirector>();
        evidenceBoardDirector.transform.SetParent(root.transform);
        SetObject(evidenceBoardDirector, "evidence", evidenceDirector);
        SetArray(evidenceBoardDirector, "theories",
            LoadAll<TheoryDefinition>().OrderBy(item => item.Id, StringComparer.Ordinal)
                .Cast<UnityEngine.Object>().ToArray());

        Canvas world = CreateCanvas("WorldCanvas", root.transform);
        world.sortingOrder = 0;
        RectTransform worldFrame = CreateLayer("WorldFrame", world.transform);
        Image background = CreateLayer("BackgroundLayer", worldFrame).gameObject.AddComponent<Image>();
        AspectRatioFitter backgroundAspect = background.gameObject.AddComponent<AspectRatioFitter>();
        backgroundAspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        background.raycastTarget = false;
        RectTransform characterLayer = CreateLayer("CharacterLayer", worldFrame);
        RectTransform hotspotLayer = CreateLayer("HotspotLayer", worldFrame);

        Canvas ui = CreateCanvas("UICanvas", root.transform);
        ui.sortingOrder = 100;
        RectTransform uiFrame = CreateLayer("UIFrame", ui.transform);
        RectTransform screenHost = CreateLayer("ScreenHost", uiFrame);
        RectTransform transitionRoot = CreateLayer("TransitionOverlay", ui.transform);
        Image transitionBlocker = transitionRoot.gameObject.AddComponent<Image>();
        transitionBlocker.color = Color.clear;
        CanvasGroup transitionGroup = transitionRoot.gameObject.AddComponent<CanvasGroup>();
        transitionGroup.alpha = 0f;
        transitionGroup.interactable = false;
        transitionGroup.blocksRaycasts = false;
        UIInputBlocker blocker = transitionRoot.gameObject.AddComponent<UIInputBlocker>();
        SetObject(blocker, "group", transitionGroup);
        FadeTransitionPlayer fade = transitionRoot.gameObject.AddComponent<FadeTransitionPlayer>();
        SetObject(fade, "overlay", transitionGroup);

        List<ScreenBase> screens = InstantiateScreens(screenHost);
        ScreenRouter router = new GameObject("ScreenRouter").AddComponent<ScreenRouter>();
        router.transform.SetParent(root.transform);
        SetArray(router, "screens", screens.Cast<UnityEngine.Object>().ToArray());
        ModalRouter modals = BuildModalRouter(uiFrame);
        SaveSlotScreen saveSlotScreenView = screens.OfType<SaveSlotScreen>().FirstOrDefault();
        if (saveSlotScreenView != null)
            SetObject(saveSlotScreenView, "modals", modals);
        foreach (ScreenBase screen in screens)
        {
            if (screen is TitleScreen || screen is SettingsScreen || screen is CreditsScreen)
                SetObject(screen, "screens", router);
            if (screen is MapScreen mapScreen)
            {
                SetObject(mapScreen, "screens", router);
                SetObject(mapScreen, "state", state);
            }
            if (screen is InvestigationRecordScreen recordScreen)
            {
                SetObject(recordScreen, "screens", router);
                SetObject(recordScreen, "evidence", evidenceDirector);
            }
            if (screen is EvidenceBoardScreen evidenceBoardScreen)
            {
                SetObject(evidenceBoardScreen, "screens", router);
                SetObject(evidenceBoardScreen, "board", evidenceBoardDirector);
            }
            if (screen is PuzzleScreen puzzleScreenView)
                SetObject(puzzleScreenView, "screens", router);
        }
        BuildPersistentHud(uiFrame, router, state);
        BuildEvidenceDiscoveryPresenter(uiFrame, evidenceDirector, evidenceBoardDirector);
        transitionRoot.SetAsLastSibling();

        CharacterView characterPrefab = AssetDatabase.LoadAssetAtPath<CharacterView>(PrefabRoot + "/Characters/PF_CharacterView.prefab");
        CharacterStage characterStage = new GameObject("CharacterStage").AddComponent<CharacterStage>();
        characterStage.transform.SetParent(root.transform);
        SetObject(characterStage, "prefab", characterPrefab);
        SetObject(characterStage, "root", characterLayer);
        SetObject(characterStage, "defaultPresentation",
            AssetDatabase.LoadAssetAtPath<CharacterPresentationProfile>(
                ContentRoot + "/Characters/CHR_PRESENTATION_STANDARD.asset"));

        LocationPresenter location = new GameObject("LocationPresenter").AddComponent<LocationPresenter>();
        location.transform.SetParent(root.transform);
        SetObject(location, "background", background);
        SetObject(location, "backgroundAspect", backgroundAspect);
        SetObject(location, "state", state);
        SetObject(location, "defaultAmbientParticles",
            AssetDatabase.LoadAssetAtPath<AmbientParticleProfile>(
                ContentRoot + "/UI/UI_AMBIENCE_LOCATION.asset"));

        AudioDirector audio = CreateAudioDirector(
            root.transform,
            out VoiceController voice,
            out SfxController sfx
        );
        InteractionFeedbackService interactionFeedback =
            new GameObject("InteractionFeedbackService").AddComponent<InteractionFeedbackService>();
        interactionFeedback.transform.SetParent(root.transform);
        SetObject(interactionFeedback, "interactiveCursor",
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                ProjectRoot + "/Art/UI/Cursors/UI_cursor_magic_arrow.png"));
        SetObject(interactionFeedback, "sfx", sfx);
        SetObject(interactionFeedback, "hoverClip", FindAudio("UIWhoop_02"));
        SetObject(interactionFeedback, "clickClip", FindAudio("FingerSnap"));
        TransitionDirector transitions = new GameObject("TransitionDirector").AddComponent<TransitionDirector>();
        transitions.transform.SetParent(root.transform);
        SetArray(transitions, "players", new UnityEngine.Object[] { fade });
        SetObject(transitions, "blocker", blocker);
        SetObject(transitions, "sfx", sfx);
        SetObject(router, "transitionDirector", transitions);
        SetObject(router, "defaultTransition", FindTransition("TRANS_FADE_STANDARD"));
        SetScreenTransitions(router,
            (ScreenId.Investigation, FindTransition("TRANS_INVESTIGATION_OPEN")),
            (ScreenId.InvestigationRecord, FindTransition("TRANS_DISCOVERY")),
            (ScreenId.EvidenceBoard, FindTransition("TRANS_DISCOVERY")),
            (ScreenId.Puzzle, FindTransition("TRANS_PUZZLE_OPEN")),
            (ScreenId.Ending, FindTransition("TRANS_ENDING")),
            (ScreenId.Map, FindTransition("TRANS_SCREEN_PUSH_RIGHT")));

        UiFeedbackInstaller feedback = uiFrame.gameObject.AddComponent<UiFeedbackInstaller>();
        SetObject(feedback, "sfx", sfx);
        SetObject(feedback, "hoverClip", FindAudio("UIWhoop_02"));
        SetObject(feedback, "clickClip", FindAudio("FingerSnap"));

        NarrativeDirector narrative = new GameObject("NarrativeDirector").AddComponent<NarrativeDirector>();
        narrative.transform.SetParent(root.transform);
        SetObject(characterStage, "narrative", narrative);
        SetObject(narrative, "state", state);
        SetObject(narrative, "screens", router);
        SetObject(narrative, "voice", voice);
        SetObject(narrative, "transitions", transitions);
        SetObject(narrative, "dialogueOpenTransition", FindTransition("TRANS_DIALOGUE_OPEN"));
        SetObject(narrative, "dialogueCloseTransition", FindTransition("TRANS_DIALOGUE_CLOSE"));
        DialogueScreen dialogueScreen = screens.OfType<DialogueScreen>().FirstOrDefault();
        if (dialogueScreen != null)
        {
            SetObject(dialogueScreen, "narrative", narrative);
            SetObject(dialogueScreen, "sfx", sfx);
        }

        PuzzleDirector puzzles = CreatePuzzleDirector(root.transform, router);
        SetObject(puzzles, "puzzleScreen", screens.OfType<PuzzleScreen>().FirstOrDefault());
        InteractionDirector interactions = new GameObject("InteractionDirector").AddComponent<InteractionDirector>();
        interactions.transform.SetParent(root.transform);
        SetObject(interactions, "state", state);
        SetObject(interactions, "narrative", narrative);
        SetObject(interactions, "puzzles", puzzles);
        SetObject(characterStage, "interactions", interactions);

        SequenceDirector sequences = new GameObject("SequenceDirector").AddComponent<SequenceDirector>();
        sequences.transform.SetParent(root.transform);
        CinematicOverlayPresenter cinematicOverlay =
            sequences.gameObject.AddComponent<CinematicOverlayPresenter>();
        SetObject(sequences, "state", state);
        SetObject(sequences, "narrative", narrative);
        SetObject(sequences, "audioDirector", audio);
        SetObject(sequences, "transitions", transitions);
        SetObject(sequences, "screens", router);
        SetObject(sequences, "cinematicOverlay", cinematicOverlay);

        StorySceneDirector story = new GameObject("StorySceneDirector").AddComponent<StorySceneDirector>();
        story.transform.SetParent(root.transform);
        SetObject(story, "state", state);
        SetObject(story, "locations", location);
        SetObject(story, "characters", characterStage);
        SetObject(story, "interactions", interactions);
        SetObject(story, "narrative", narrative);
        SetObject(story, "audioDirector", audio);
        SetObject(story, "screens", router);
        SetObject(story, "transitions", transitions);
        SetObject(story, "sequences", sequences);

        GameFlowController flow = new GameObject("GameFlowController").AddComponent<GameFlowController>();
        flow.transform.SetParent(root.transform);
        SetObject(flow, "content", AssetDatabase.LoadAssetAtPath<ContentDatabase>(ContentRoot + "/Game/DATABASE_Content.asset"));
        SetObject(flow, "scenes", story);
        SetObject(flow, "state", state);
        SetObject(interactions, "flow", flow);
        GameObject hotspotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            ProjectRoot + "/Prefabs/Interaction/PF_Hotspot.prefab");
        SetObject(
            interactions,
            "hotspotPrefab",
            hotspotPrefab != null ? hotspotPrefab.GetComponent<InteractionPointView>() : null);
        SetObject(interactions, "hotspotRoot", hotspotLayer);

        SaveCheckpoint saveCheckpoint = root.AddComponent<SaveCheckpoint>();
        SetObject(saveCheckpoint, "stateStore", state);
        SetObject(saveCheckpoint, "storyScenes", story);
        SetObject(saveCheckpoint, "flow", flow);

        GameStartup startup = root.AddComponent<GameStartup>();
        SetObject(startup, "flow", flow);
        SetObject(startup, "screens", router);
        SetObject(startup, "titleScreen", screens.OfType<TitleScreen>().FirstOrDefault());
        SetObject(startup, "saveSlotScreen", screens.OfType<SaveSlotScreen>().FirstOrDefault());
        SetObject(startup, "state", state);
        SetObject(startup, "saveCheckpoint", saveCheckpoint);
        SetObject(startup, "audioDirector", audio);
        SetObject(
            startup,
            "titleAudio",
            AssetDatabase.LoadAssetAtPath<AudioCueProfile>(ContentRoot + "/Audio/AUDIO_TITLE.asset")
        );
        SettingsScreen settingsScreen = screens.OfType<SettingsScreen>().FirstOrDefault();
        if (settingsScreen != null)
            SetObject(settingsScreen, "audioDirector", audio);
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule))
            .transform.SetParent(root.transform);
        EditorSceneManager.SaveScene(scene, ProjectRoot + "/Scenes/Game.unity");
    }

    private static AudioDirector CreateAudioDirector(
        Transform parent,
        out VoiceController voice,
        out SfxController sfx)
    {
        GameObject root = new("AudioDirector");
        root.transform.SetParent(parent);
        MusicController music = AddAudioController<MusicController>("Music", root.transform, "source");
        AmbienceController ambience = new GameObject("Ambience").AddComponent<AmbienceController>();
        ambience.transform.SetParent(root.transform);
        SetObject(ambience, "sourceA", ambience.gameObject.AddComponent<AudioSource>());
        SetObject(ambience, "sourceB", ambience.gameObject.AddComponent<AudioSource>());
        sfx = AddAudioController<SfxController>("SFX", root.transform, "source");
        SetObject(sfx, "loopSource", sfx.gameObject.AddComponent<AudioSource>());
        voice = AddAudioController<VoiceController>("Voice", root.transform, "source");
        AudioDirector director = root.AddComponent<AudioDirector>();
        SetObject(director, "music", music);
        SetObject(director, "ambience", ambience);
        SetObject(director, "sfx", sfx);
        return director;
    }

    private static T AddAudioController<T>(string name, Transform parent, string field)
        where T : Component
    {
        GameObject gameObject = new(name);
        gameObject.transform.SetParent(parent);
        AudioSource source = gameObject.AddComponent<AudioSource>();
        T controller = gameObject.AddComponent<T>();
        SetObject(controller, field, source);
        return controller;
    }

    private static PuzzleDirector CreatePuzzleDirector(Transform parent, ScreenRouter router)
    {
        GameObject root = new("PuzzleDirector");
        root.transform.SetParent(parent);
        PuzzleDirector director = root.AddComponent<PuzzleDirector>();
        Type[] types =
        {
            typeof(AudioRestorationPuzzleController), typeof(BloodPatternPuzzleController),
            typeof(CCTVLogPuzzleController), typeof(CargoRailPuzzleController),
            typeof(CauseOfDeathPuzzleController), typeof(ClaireContradictionPuzzleController),
            typeof(DNAPuzzleController), typeof(FinalAccusationController), typeof(LuminolPuzzleController),
            typeof(StabilizerPuzzleController), typeof(StairPuzzleController),
            typeof(TimelinePuzzleController), typeof(VaultAuthPuzzleController),
        };
        List<UnityEngine.Object> controllers = new();
        foreach (Type type in types)
            controllers.Add(root.AddComponent(type));
        SetArray(director, "controllers", controllers.ToArray());
        SetObject(director, "screens", router);
        return director;
    }

    private static List<ScreenBase> InstantiateScreens(RectTransform parent)
    {
        List<ScreenBase> result = new();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabRoot + "/UI" }))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab == null || prefab.GetComponent<ScreenBase>() == null)
                continue;
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false);
            result.Add(instance.GetComponent<ScreenBase>());
        }
        return result;
    }

    private static Canvas CreateCanvas(string name, Transform parent)
    {
        GameObject gameObject = new(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        gameObject.transform.SetParent(parent);
        Canvas canvas = gameObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static ModalRouter BuildModalRouter(Transform parent)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        GameObject routerObject = new("ModalRouter");
        routerObject.transform.SetParent(parent, false);
        ModalRouter router = routerObject.AddComponent<ModalRouter>();

        GameObject confirmObject = new("ConfirmModal", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(ConfirmDialog));
        confirmObject.transform.SetParent(parent, false);
        SetRect((RectTransform)confirmObject.transform, Vector2.zero, Vector2.one);
        Image dim = confirmObject.GetComponent<Image>();
        dim.color = new Color(0.005f, 0.008f, 0.015f, 0.82f);

        Image panel = CreateLayer("Confirm Panel", confirmObject.transform)
            .gameObject.AddComponent<Image>();
        panel.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        panel.type = Image.Type.Sliced;
        panel.color = new Color(0.06f, 0.08f, 0.11f, 0.99f);
        SetRect(panel.rectTransform, new Vector2(0.30f, 0.32f), new Vector2(0.70f, 0.68f));

        Text message = CreateText("Message", panel.transform, font, 28, TextAnchor.MiddleCenter);
        message.color = new Color(0.965f, 0.941f, 0.867f, 1f);
        SetRect(message.rectTransform, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.88f));
        Button cancel = CreateTitleButton("CancelButton", panel.transform, font, "취소", false);
        SetRect((RectTransform)cancel.transform,
            new Vector2(0.08f, 0.10f), new Vector2(0.46f, 0.30f));
        Button confirm = CreateTitleButton("ConfirmButton", panel.transform, font, "확인", true);
        SetRect((RectTransform)confirm.transform,
            new Vector2(0.54f, 0.10f), new Vector2(0.92f, 0.30f));

        ConfirmDialog dialog = confirmObject.GetComponent<ConfirmDialog>();
        SetObject(dialog, "messageLabel", message);
        SetObject(dialog, "confirmButton", confirm);
        SetObject(dialog, "cancelButton", cancel);
        SetObject(router, "confirm", confirmObject);
        SetObject(router, "confirmDialog", dialog);
        confirmObject.SetActive(false);
        return router;
    }

    private static void BuildEvidenceDiscoveryPresenter(
        RectTransform parent, EvidenceDirector evidence, EvidenceBoardDirector board)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        RectTransform root = CreateLayer("EvidenceDiscoveryOverlay", parent);
        CanvasGroup group = root.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;
        Image panel = CreateLayer("Card", root).gameObject.AddComponent<Image>();
        panel.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ThemePanelPath);
        panel.type = Image.Type.Sliced; panel.color = new Color(0.055f, 0.07f, 0.085f, 0.98f);
        SetRect(panel.rectTransform, new Vector2(0.31f, 0.31f), new Vector2(0.69f, 0.69f));
        Image image = CreateLayer("Evidence Image", panel.transform).gameObject.AddComponent<Image>();
        image.preserveAspect = true; image.raycastTarget = false;
        SetRect(image.rectTransform, new Vector2(0.07f, 0.25f), new Vector2(0.38f, 0.84f));
        Text heading = CreateText("Heading", panel.transform, font, 17, TextAnchor.MiddleLeft);
        heading.text = "NEW EVIDENCE"; heading.color = new Color(0.80f, 0.61f, 0.30f, 1f);
        SetRect(heading.rectTransform, new Vector2(0.42f, 0.72f), new Vector2(0.93f, 0.86f));
        Text title = CreateText("Title", panel.transform, font, 27, TextAnchor.MiddleLeft);
        title.color = new Color(0.965f, 0.827f, 0.529f, 1f);
        SetRect(title.rectTransform, new Vector2(0.42f, 0.53f), new Vector2(0.93f, 0.72f));
        Text description = CreateText("Description", panel.transform, font, 17, TextAnchor.UpperLeft);
        description.color = new Color(0.86f, 0.83f, 0.77f, 1f);
        description.horizontalOverflow = HorizontalWrapMode.Wrap;
        SetRect(description.rectTransform, new Vector2(0.42f, 0.18f), new Vector2(0.93f, 0.52f));
        EvidenceDiscoveryPresenter presenter = root.gameObject.AddComponent<EvidenceDiscoveryPresenter>();
        SetObject(presenter, "evidence", evidence); SetObject(presenter, "group", group);
        SetObject(presenter, "board", board); SetObject(presenter, "heading", heading);
        SetObject(presenter, "image", image); SetObject(presenter, "title", title);
        SetObject(presenter, "description", description);
        SetObject(presenter, "profile", FindTransition("TRANS_DISCOVERY"));
    }

    private static void BuildPersistentHud(
        Transform parent,
        ScreenRouter router,
        GameStateStore state)
    {
        Font font = LoadUiFont("Pretendard/FONT_Pretendard-SemiBold.ttf");
        GameObject root = new("PersistentHUD", typeof(RectTransform), typeof(PersistentHud));
        root.transform.SetParent(parent, false);
        SetRect((RectTransform)root.transform, Vector2.zero, Vector2.one);

        Image topBar = CreateLayer("StatusBar", root.transform).gameObject.AddComponent<Image>();
        topBar.color = new Color(0.018f, 0.035f, 0.055f, 0.76f);
        topBar.raycastTarget = false;
        SetRect(topBar.rectTransform, new Vector2(0f, 0.84f), Vector2.one);

        Text time = CreateHudLabel(
            "Time", topBar.transform, font, 20, TextAnchor.MiddleLeft,
            "DAY 1 · 오후", 0.02f, 0.13f
        );
        Text location = CreateHudLabel(
            "Location", topBar.transform, font, 28, TextAnchor.MiddleLeft,
            "항구", 0.135f, 0.28f
        );
        Text objective = CreateHudLabel(
            "Objective", topBar.transform, font, 27, TextAnchor.MiddleCenter,
            "◆ 항구의 기자", 0.30f, 0.70f
        );

        Button map = CreateSpriteButton(
            "MapButton", root.transform, font, "지도",
            ProjectRoot + "/Art/UI/Buttons/UI_btn_standard_normal.png",
            ProjectRoot + "/Art/UI/Buttons/UI_btn_standard_pressed.png", out _);
        SetRect((RectTransform)map.transform, new Vector2(0.74f, 0.87f), new Vector2(0.85f, 0.95f));

        Button record = CreateSpriteButton(
            "RecordButton", root.transform, font, "수사 기록",
            ProjectRoot + "/Art/UI/Buttons/UI_btn_standard_normal.png",
            ProjectRoot + "/Art/UI/Buttons/UI_btn_standard_pressed.png", out _);
        SetRect((RectTransform)record.transform, new Vector2(0.865f, 0.87f), new Vector2(0.985f, 0.95f));

        PersistentHud hud = root.GetComponent<PersistentHud>();
        SetObject(hud, "screens", router);
        SetObject(hud, "state", state);
        SetObject(hud, "content", AssetDatabase.LoadAssetAtPath<ContentDatabase>(
            ContentRoot + "/Game/DATABASE_Content.asset"));
        SetObject(hud, "mapButton", map);
        SetObject(hud, "recordButton", record);
        SetObject(hud, "timeLabel", time);
        SetObject(hud, "locationLabel", location);
        SetObject(hud, "objectiveLabel", objective);
    }

    private static Text CreateHudLabel(
        string name,
        Transform parent,
        Font font,
        int size,
        TextAnchor alignment,
        string value,
        float minX,
        float maxX)
    {
        Image panel = CreateLayer(name + "Panel", parent).gameObject.AddComponent<Image>();
        panel.color = new Color(0.055f, 0.10f, 0.14f, 0.78f);
        panel.raycastTarget = false;
        SetRect(panel.rectTransform, new Vector2(minX, 0.08f), new Vector2(maxX, 0.92f));
        Text label = CreateText(name + "Label", panel.transform, font, size, alignment);
        label.text = value;
        label.color = new Color(0.91f, 0.87f, 0.76f, 1f);
        SetRect(label.rectTransform, new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.92f));
        return label;
    }

    private static Image CreateHudMeter(
        string name,
        Transform parent,
        float minX,
        float maxX,
        Color color)
    {
        RectTransform panel = parent.Find(
            name.StartsWith("Anxiety") ? "AnxietyPanel" : "IntegrityPanel"
        ) as RectTransform;
        GameObject trackObject = new(name + "Track", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        trackObject.transform.SetParent(panel, false);
        Image track = trackObject.GetComponent<Image>();
        track.color = new Color(0.018f, 0.027f, 0.043f, 0.82f);
        track.raycastTarget = false;
        SetRect(track.rectTransform, new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.28f));
        GameObject fillObject = new(name + "Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObject.transform.SetParent(trackObject.transform, false);
        Image fill = fillObject.GetComponent<Image>();
        fill.color = color;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.raycastTarget = false;
        SetRect(fill.rectTransform, Vector2.zero, Vector2.one);
        return fill;
    }

    private static Font LoadUiFont(string relativePath) =>
        AssetDatabase.LoadAssetAtPath<Font>(ProjectRoot + "/Art/UI/Fonts/" + relativePath)
        ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    private static RectTransform CreateLayer(string name, Transform parent)
    {
        GameObject layer = new(name, typeof(RectTransform));
        layer.transform.SetParent(parent, false);
        RectTransform rect = layer.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static bool TryGetScreenType(string name, out Type type, out ScreenId id)
    {
        Dictionary<string, (Type type, ScreenId id)> map = new()
        {
            ["PF_TitleScreen"] = (typeof(TitleScreen), ScreenId.Title),
            ["PF_SaveSlotScreen"] = (typeof(SaveSlotScreen), ScreenId.SaveSlot),
            ["PF_ExplorationScreen"] = (typeof(ExplorationScreen), ScreenId.Exploration),
            ["PF_DialogueScreen"] = (typeof(DialogueScreen), ScreenId.Dialogue),
            ["PF_MapScreen"] = (typeof(MapScreen), ScreenId.Map),
            ["PF_InvestigationScreen"] = (typeof(InvestigationScreen), ScreenId.Investigation),
            ["PF_RecordScreen"] = (typeof(InvestigationRecordScreen), ScreenId.InvestigationRecord),
            ["PF_InterrogationScreen"] = (typeof(InterrogationScreen), ScreenId.Interrogation),
            ["PF_EvidenceBoardScreen"] = (typeof(EvidenceBoardScreen), ScreenId.EvidenceBoard),
            ["PF_ReconstructionScreen"] = (typeof(ReconstructionScreen), ScreenId.Reconstruction),
            ["PF_PuzzleScreen"] = (typeof(PuzzleScreen), ScreenId.Puzzle),
            ["PF_EndingScreen"] = (typeof(EndingScreen), ScreenId.Ending),
            ["PF_SettingsScreen"] = (typeof(SettingsScreen), ScreenId.Settings),
            ["PF_CreditsScreen"] = (typeof(CreditsScreen), ScreenId.Credits),
        };
        if (map.TryGetValue(name, out var value))
        {
            type = value.type;
            id = value.id;
            return true;
        }
        type = null;
        id = default;
        return false;
    }

    private static void ConfigureAudio(AudioCueProfile profile, string token)
    {
        profile.musicVolume = 0.65f;
        profile.ambienceAVolume = 0.6f;
        profile.ambienceBVolume = 0.4f;
        profile.crossfadeDuration = 1.2f;
        profile.music = FindAudio(token.Contains("HORIZON") ? "Horizon_Room" : token.Contains("PORT") ? "Passage_to_Port" : "Mystery");
        profile.ambienceA = FindAudio(token.Contains("PORT") ? "waves" : token.Contains("ENGINE") ? "engine" : "wind");
        EditorUtility.SetDirty(profile);
    }

    private static void ConfigureTexture(string path, bool sliced)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.spriteBorder = sliced ? new Vector4(90, 90, 90, 90) : Vector4.zero;
        importer.SaveAndReimport();
    }

    private static void ConfigureRuntimeVisualQuality()
    {
        string[] folders =
        {
            ProjectRoot + "/Art/Backgrounds",
            ProjectRoot + "/Art/Characters",
            ProjectRoot + "/Art/UI",
        };
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", folders))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                continue;
            bool dirty = importer.textureCompression != TextureImporterCompression.Uncompressed
                || importer.maxTextureSize < 4096
                || importer.mipmapEnabled;
            if (!dirty)
                continue;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.maxTextureSize = 4096;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
    }

    private static void ReplaceAsset(string path, ScriptableObject asset)
    {
        string absolute = Path.GetFullPath(path);
        if (File.Exists(absolute))
            File.Delete(absolute);
        AssetDatabase.CreateAsset(asset, path);
    }

    private static T GetOrCreate<T>(string path)
        where T : ScriptableObject
    {
        T existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
            return existing;
        EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
        T created = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(created, path);
        return created;
    }

    private static void SavePrefab(string path, GameObject root)
    {
        string absolute = Path.GetFullPath(path);
        if (File.Exists(absolute) && new FileInfo(absolute).Length == 0)
            File.Delete(absolute);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void SetString(UnityEngine.Object target, string field, string value)
    {
        if (target == null)
            return;
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property != null)
            property.stringValue = value ?? string.Empty;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetObject(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        if (target == null)
            return;
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property != null)
            property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetBool(UnityEngine.Object target, string field, bool value)
    {
        if (target == null)
            return;
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property != null)
            property.boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetFloat(UnityEngine.Object target, string field, float value)
    {
        if (target == null)
            return;
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property != null)
            property.floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetEnum(UnityEngine.Object target, string field, int value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(field).enumValueIndex = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetArray(UnityEngine.Object target, string field, UnityEngine.Object[] values)
    {
        if (target == null)
            return;
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
            return;
        property.arraySize = values.Length;
        for (int index = 0; index < values.Length; index++)
            property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetScreenTransitions(
        ScreenRouter router,
        params (ScreenId screen, TransitionProfile profile)[] routes)
    {
        SerializedObject serialized = new(router);
        SerializedProperty property = serialized.FindProperty("transitionRoutes");
        property.arraySize = routes.Length;
        for (var index = 0; index < routes.Length; index++)
        {
            SerializedProperty route = property.GetArrayElementAtIndex(index);
            route.FindPropertyRelative("screen").enumValueIndex = (int)routes[index].screen;
            route.FindPropertyRelative("profile").objectReferenceValue = routes[index].profile;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(router);
    }

    private static List<T> LoadAll<T>()
        where T : UnityEngine.Object =>
        AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { ContentRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(item => item != null)
            .OrderBy(item => item.name, StringComparer.Ordinal)
            .ToList();

    private static Sprite FindSprite(string token) => FindAsset<Sprite>(token, ProjectRoot + "/Art");
    private static AudioClip FindAudio(string token) => FindAsset<AudioClip>(token, ProjectRoot + "/Audio");

    private static T FindAsset<T>(string token, string folder)
        where T : UnityEngine.Object
    {
        string normalized = NormalizeSearch(token);
        foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (NormalizeSearch(Path.GetFileNameWithoutExtension(path)).Contains(normalized))
                return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        return null;
    }

    private static Sprite FindLocationSprite(string token)
    {
        Dictionary<string, string> aliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["RICHARD_SUITE"] = "richard_suite", ["SERVICE_RAIL"] = "service_rail",
            ["ENGINE_CONTROL"] = "engine_control", ["NEWS_LOUNGE"] = "news_lounge",
            ["STAIR_B"] = "crew_stairs", ["HORIZON"] = "horizon_room",
            ["BALLAST"] = "ballast", ["GANGWAY"] = "gangway", ["PORT"] = "port",
            ["CLAIRE_CABIN"] = "cabin_claire", ["DANIEL_CABIN"] = "cabin_daniel",
            ["STERN"] = "open_deck",
        };
        return FindSprite(aliases.TryGetValue(token, out string alias) ? alias : token);
    }

    private static Sprite FindCharacterSprite(string token)
    {
        string alias = token.ToLowerInvariant()
            .Replace("chr_", string.Empty)
            .Replace("_f01", string.Empty)
            .Replace("_m01", string.Empty);
        Dictionary<string, string> worldNames = new()
        {
            ["adrian"] = "adrian_vale",
            ["claire"] = "claire_hawthorne",
            ["daniel"] = "daniel_mercer",
            ["evelyn"] = "evelyn_shaw",
            ["helena"] = "helena_ward",
            ["marcus"] = "marcus_bell",
            ["owen"] = "owen_price",
            ["richard"] = "richard_hawthorne",
            ["thomas"] = "thomas_reed",
        };
        string worldName = worldNames.TryGetValue(alias, out string canonicalName)
            ? canonicalName
            : alias;
        string path = $"{ProjectRoot}/Art/Characters/World/CHR_{worldName}.png";
        Sprite worldSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return worldSprite != null ? worldSprite : FindSprite("CHR_" + alias);
    }

    private static CharacterDefinition FindCharacter(string name)
    {
        string normalized = NormalizeSearch(name);
        Dictionary<string, string> aliases = new()
        {
            ["아드리안"] = "ADRIAN", ["클레어"] = "CLAIRE", ["다니엘"] = "DANIEL",
            ["이블린"] = "EVELYN", ["헬레나"] = "HELENA", ["마커스"] = "MARCUS",
            ["오언"] = "OWEN", ["리처드"] = "RICHARD", ["토머스"] = "THOMAS",
        };
        string token = aliases.FirstOrDefault(pair => normalized.Contains(NormalizeSearch(pair.Key))).Value;
        if (string.IsNullOrEmpty(token))
            token = normalized;
        return LoadAll<CharacterDefinition>().FirstOrDefault(item => NormalizeSearch(item.name).Contains(NormalizeSearch(token)));
    }

    private static LocationDefinition FindLocation(string source)
    {
        string normalized = NormalizeSearch(source);
        Dictionary<string, string> aliases = new()
        {
            ["SUITE"] = "RICHARD_SUITE", ["ATRIUM"] = "ATRIUM", ["DINING"] = "DINING",
            ["BALLROOM"] = "BALLROOM", ["HORIZON"] = "HORIZON", ["MEDBAY"] = "MEDBAY",
            ["SECURITY"] = "SECURITY", ["VAULT"] = "VAULT", ["ARCHIVE"] = "ARCHIVE",
            ["BALLAST"] = "BALLAST", ["STAIR"] = "STAIR_B", ["RAIL"] = "SERVICE_RAIL",
            ["ENGINE"] = "ENGINE_CONTROL", ["NEWS"] = "NEWS_LOUNGE", ["PROMENADE"] = "PROMENADE",
            ["GANGWAY"] = "GANGWAY", ["PORT"] = "PORT", ["INTERVIEW"] = "INTERVIEW",
        };
        string token = aliases.FirstOrDefault(pair => normalized.Contains(pair.Key)).Value ?? "HORIZON";
        return LoadAll<LocationDefinition>().FirstOrDefault(item => item.name.Equals("LOC_" + token, StringComparison.OrdinalIgnoreCase));
    }

    private static TransitionProfile FindTransition(string name) =>
        LoadAll<TransitionProfile>().FirstOrDefault(item => item.name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static PuzzleDefinition FindPuzzleForScene(string sceneId) =>
        LoadAll<PuzzleDefinition>().FirstOrDefault(item => item.name.Contains(NormalizeId(sceneId), StringComparison.OrdinalIgnoreCase));

    private static List<SceneRow> ReadSceneRows() => ReadCsv(SceneIndexPath)
        .Skip(1)
        .Where(row => row.Count >= 9 && !string.IsNullOrWhiteSpace(row[0]))
        .Select(row => new SceneRow
        {
            Id = row[0], Chapter = row[1], Title = row[2], Time = row[3],
            Location = row[4], Next = row[7], Characters = row[8],
        })
        .ToList();

    private static List<List<string>> ReadCsv(string assetPath)
    {
        List<List<string>> rows = new();
        foreach (string line in File.ReadAllLines(Path.GetFullPath(assetPath)))
        {
            List<string> fields = new();
            bool quoted = false;
            System.Text.StringBuilder field = new();
            for (int index = 0; index < line.Length; index++)
            {
                char character = line[index];
                if (character == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                        quoted = !quoted;
                }
                else if (character == ',' && !quoted)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                    field.Append(character);
            }
            fields.Add(field.ToString());
            rows.Add(fields);
        }
        return rows;
    }

    private static string[] SplitCharacters(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "없음")
            return Array.Empty<string>();
        if (value.Contains("전원"))
            return new[] { "아드리안", "클레어", "이블린", "헬레나" };
        return value.Split(new[] { ';', '/', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
    }

    private static int ParseDay(string id) => id.StartsWith("P-") ? 0 : int.TryParse(id.AsSpan(1, 1), out int day) ? day : 0;

    private static int ParseTimeBlock(string value)
    {
        if (!TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan time))
            return (int)TimeBlock.Unknown;
        if (time.Hours < 12)
            return (int)TimeBlock.Morning;
        if (time.Hours < 17)
            return (int)TimeBlock.Afternoon;
        if (time.Hours < 21)
            return (int)TimeBlock.Evening;
        return (int)TimeBlock.Night;
    }

    private static string FirstNextScene(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains("엔딩"))
            return string.Empty;
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(value, @"(?:P|D\d)-\d{2}");
        return match.Success ? match.Value : string.Empty;
    }

    private static string DialogueAssetPath(string id)
    {
        string group = id.StartsWith("P-") ? "Prologue" : $"Day{id[1]}".Replace("Day1", "Day01").Replace("Day2", "Day02").Replace("Day3", "Day03").Replace("Day4", "Day04").Replace("Day5", "Day05").Replace("Day6", "Day06").Replace("Day7", "Day07").Replace("Day8", "Day08");
        string folder = $"{ContentRoot}/Dialogue/{group}";
        EnsureFolder(folder);
        return $"{folder}/DIA_{NormalizeId(id)}.asset";
    }

    private static string PlacementPath(string id) => $"{ContentRoot}/Characters/PlacementSets/Generated/SET_{NormalizeId(id)}_CHARACTERS.asset";
    private static string InteractionPath(string id) => $"{ContentRoot}/Locations/InteractionSets/Generated/INT_{NormalizeId(id)}.asset";
    private static string NormalizeId(string value) => value.Replace("-", "_");
    private static string NormalizeIdFromName(string value)
    {
        if (
            value.Length >= 3
            && value[0] == 'P'
            && char.IsDigit(value[1])
            && char.IsDigit(value[2])
        )
            return $"P_{value.Substring(1, 2)}";

        string[] parts = value.Split('_');
        return parts.Length >= 2 ? string.Join("_", parts.Take(2)) : value;
    }
    private static string NormalizeSearch(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
    private static string Humanize(string value) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.Replace('_', ' ').ToLowerInvariant());

    private static void EnsureFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            return;
        string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }

    private static string ToAssetPath(string absolutePath) => absolutePath.Replace('\\', '/').Substring(Directory.GetCurrentDirectory().Replace('\\', '/').Length + 1);
}

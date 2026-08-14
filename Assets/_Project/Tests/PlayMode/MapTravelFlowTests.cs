using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class MapTravelFlowTests
{
    [UnityTest]
    public IEnumerator MapTravelAdvanceRejectsWrongDestinationAndEntersTheExactTarget()
    {
        using var harness = new FlowHarness();
        int checkpointCount = 0;
        harness.Flow.ProgressCheckpointReached += () => checkpointCount++;

        yield return Await(harness.Flow.StartAsync(harness.Source.Id));
        yield return Await(harness.Flow.AdvanceAsync());

        Assert.That(harness.State.IsSceneCompleted(harness.Source.Id), Is.True);
        Assert.That(harness.State.State.currentStorySceneId, Is.EqualTo(harness.Source.Id));
        Assert.That(harness.State.State.currentLocationId, Is.EqualTo(harness.SourceLocation.Id));
        Assert.That(harness.State.State.pendingStorySceneId, Is.EqualTo(harness.Target.Id));
        Assert.That(harness.StoryScenes.Current, Is.SameAs(harness.Source));
        Assert.That(checkpointCount, Is.EqualTo(1));
        Assert.That(harness.Flow.TryGetPendingTravel(out PendingStorySceneTravel pending), Is.True);
        Assert.That(pending.SourceScene, Is.SameAs(harness.Source));
        Assert.That(pending.TargetScene, Is.SameAs(harness.Target));
        Assert.That(pending.DestinationId, Is.EqualTo(harness.TargetLocation.Id));

        Assert.That(harness.Flow.CanTravelTo("LOC_WRONG", out string reason), Is.False);
        Assert.That(reason, Does.Contain("not the pending destination"));
        Task<StorySceneTravelResult> wrongTravel = harness.Flow.TravelAsync("LOC_WRONG");
        yield return Await(wrongTravel);
        Assert.That(wrongTravel.Result.Success, Is.False);
        Assert.That(harness.State.State.currentStorySceneId, Is.EqualTo(harness.Source.Id));
        Assert.That(harness.State.State.currentLocationId, Is.EqualTo(harness.SourceLocation.Id));
        Assert.That(harness.State.State.pendingStorySceneId, Is.EqualTo(harness.Target.Id));

        Assert.That(harness.Flow.CanTravelTo(harness.TargetLocation.Id), Is.True);
        Task<StorySceneTravelResult> validTravel = harness.Flow.TravelAsync(
            harness.TargetLocation.Id);
        yield return Await(validTravel);

        Assert.That(validTravel.Result.Success, Is.True, validTravel.Result.Message);
        Assert.That(harness.State.State.currentStorySceneId, Is.EqualTo(harness.Target.Id));
        Assert.That(harness.State.State.currentLocationId, Is.EqualTo(harness.TargetLocation.Id));
        Assert.That(harness.State.State.pendingStorySceneId, Is.Empty);
        Assert.That(harness.StoryScenes.Current, Is.SameAs(harness.Target));
    }

    [UnityTest]
    public IEnumerator ResumeRestoresPendingSourcePresentationWithoutReplayingEntryEffects()
    {
        using var harness = new FlowHarness(addSourceEntryTrustEffect: true);

        yield return Await(harness.Flow.StartAsync(harness.Source.Id));
        Assert.That(harness.State.GetTrust("CHR_DANIEL"), Is.EqualTo(3));
        harness.State.CompleteScene(harness.Source.Id);
        harness.State.SetPendingStoryScene(harness.Target.Id);
        harness.State.SetCurrentLocation("LOC_STALE");

        yield return Await(harness.Flow.ResumeAsync());

        Assert.That(harness.StoryScenes.Current, Is.SameAs(harness.Source));
        Assert.That(harness.State.State.currentStorySceneId, Is.EqualTo(harness.Source.Id));
        Assert.That(harness.State.State.currentLocationId, Is.EqualTo(harness.SourceLocation.Id));
        Assert.That(harness.State.State.pendingStorySceneId, Is.EqualTo(harness.Target.Id));
        Assert.That(
            harness.State.GetTrust("CHR_DANIEL"),
            Is.EqualTo(3),
            "Resume must reconstruct presentation without applying On Enter effects again.");
    }

    [UnityTest]
    public IEnumerator ResumeRepairsCompletedVersionOneMapRouteIntoPendingTravel()
    {
        using var harness = new FlowHarness(addSourceEntryTrustEffect: true);
        int checkpointCount = 0;
        harness.Flow.ProgressCheckpointReached += () => checkpointCount++;

        yield return Await(harness.Flow.StartAsync(harness.Source.Id));
        harness.State.CompleteScene(harness.Source.Id);
        harness.State.ClearPendingStoryScene();

        yield return Await(harness.Flow.ResumeAsync());

        Assert.That(harness.State.State.currentStorySceneId, Is.EqualTo(harness.Source.Id));
        Assert.That(harness.State.State.currentLocationId, Is.EqualTo(harness.SourceLocation.Id));
        Assert.That(harness.State.State.pendingStorySceneId, Is.EqualTo(harness.Target.Id));
        Assert.That(harness.StoryScenes.Current, Is.SameAs(harness.Source));
        Assert.That(harness.State.GetTrust("CHR_DANIEL"), Is.EqualTo(3));
        Assert.That(checkpointCount, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator PendingTravelProgressEventCapturesTheStableCheckpoint()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "UnderTheHorizonTests",
            "MapTravelCheckpoint",
            Guid.NewGuid().ToString("N"));
        AppServiceRegistry previousServices = AppContext.Services;

        try
        {
            var saves = new SaveService(directory);
            AppContext.Services = new AppServiceRegistry();
            AppContext.Services.Register(saves);
            using var harness = new FlowHarness();
            SaveCheckpoint checkpoint = harness.Owner.AddComponent<SaveCheckpoint>();
            SetPrivateField(checkpoint, "stateStore", harness.State);
            SetPrivateField(checkpoint, "storyScenes", harness.StoryScenes);
            SetPrivateField(checkpoint, "flow", harness.Flow);
            checkpoint.enabled = false;
            checkpoint.enabled = true;
            var slot = new SaveSlot(1);
            checkpoint.Bind(slot);

            yield return Await(harness.Flow.StartAsync(harness.Source.Id));
            yield return Await(harness.Flow.AdvanceAsync());

            GameState saved = saves.Load(slot);
            Assert.That(saved.completedStoryScenes, Does.Contain(harness.Source.Id));
            Assert.That(saved.currentStorySceneId, Is.EqualTo(harness.Source.Id));
            Assert.That(saved.currentLocationId, Is.EqualTo(harness.SourceLocation.Id));
            Assert.That(saved.pendingStorySceneId, Is.EqualTo(harness.Target.Id));
        }
        finally
        {
            AppContext.Services = previousServices;
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }

    [UnityTest]
    public IEnumerator FailedAdvanceRequestRollsBackActionEffectsAndInteractionCompletion()
    {
        var owner = new GameObject("Interaction rollback test");
        var action = ScriptableObject.CreateInstance<MutatingAdvanceTestAction>();
        var definition = ScriptableObject.CreateInstance<InteractionDefinition>();
        try
        {
            GameStateStore state = owner.AddComponent<GameStateStore>();
            InteractionDirector interactions = owner.AddComponent<InteractionDirector>();
            SetPrivateField(interactions, "state", state);
            SetPrivateField(definition, "id", "INT_TEST_ADVANCE");
            SetPrivateField(definition, "action", action);

            Task<InteractionResult> execute = interactions.ExecuteAsync(definition);
            yield return Await(execute);

            Assert.That(execute.Result.Success, Is.False);
            Assert.That(execute.Result.Message, Does.Contain("GameFlowController"));
            Assert.That(state.HasFlag(MutatingAdvanceTestAction.FlagId), Is.False);
            Assert.That(state.IsInteractionCompleted("INT_TEST_ADVANCE"), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(action);
            Object.DestroyImmediate(owner);
        }
    }

    [UnityTest]
    public IEnumerator NonrepeatableAdvanceCanSatisfyItsOwnRouteCompletionCondition()
    {
        using var harness = new FlowHarness();
        const string interactionId = "INT_TEST_ROUTE_GATE";
        var condition = ScriptableObject.CreateInstance<InteractionCompletedCondition>();
        var action = ScriptableObject.CreateInstance<StorySceneAdvanceInteractionAction>();
        var definition = ScriptableObject.CreateInstance<InteractionDefinition>();
        try
        {
            SetPrivateField(condition, "interactionId", interactionId);
            SetPrivateField(
                harness.Source.Routes[0],
                "conditions",
                new Condition[] { condition });
            SetPrivateField(definition, "id", interactionId);
            SetPrivateField(definition, "action", action);

            yield return Await(harness.Flow.StartAsync(harness.Source.Id));
            Task<InteractionResult> execute = harness.Interactions.ExecuteAsync(definition);
            yield return Await(execute);

            Assert.That(
                execute.Result.Success,
                Is.True,
                "The nonrepeatable interaction completion must be staged before route prevalidation.");
            Assert.That(harness.State.IsInteractionCompleted(interactionId), Is.True);
            Assert.That(harness.State.IsSceneCompleted(harness.Source.Id), Is.True);
            Assert.That(harness.State.State.currentStorySceneId, Is.EqualTo(harness.Source.Id));
            Assert.That(harness.State.State.currentLocationId, Is.EqualTo(harness.SourceLocation.Id));
            Assert.That(harness.State.State.pendingStorySceneId, Is.EqualTo(harness.Target.Id));
        }
        finally
        {
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(action);
            Object.DestroyImmediate(condition);
        }
    }

    [UnityTest]
    public IEnumerator StalePendingTargetOutsideTheCurrentMapRouteIsRejectedWithoutMutation()
    {
        using var harness = new FlowHarness();

        yield return Await(harness.Flow.StartAsync(harness.Source.Id));
        harness.State.CompleteScene(harness.Source.Id);
        harness.State.SetPendingStoryScene(harness.UnrelatedTarget.Id);
        GameState before = harness.State.State.Clone();

        Assert.That(
            harness.Flow.CanTravelTo(harness.UnrelatedLocation.Id, out string reason),
            Is.False);
        Assert.That(reason, Is.Not.Empty);
        Task<StorySceneTravelResult> travel = harness.Flow.TravelAsync(
            harness.UnrelatedLocation.Id);
        yield return Await(travel);

        Assert.That(travel.Result.Success, Is.False);
        Assert.That(harness.StoryScenes.Current, Is.SameAs(harness.Source));
        Assert.That(harness.State.State.currentStorySceneId, Is.EqualTo(before.currentStorySceneId));
        Assert.That(harness.State.State.currentLocationId, Is.EqualTo(before.currentLocationId));
        Assert.That(harness.State.State.pendingStorySceneId, Is.EqualTo(before.pendingStorySceneId));
        Assert.That(
            harness.State.State.completedStoryScenes,
            Is.EquivalentTo(before.completedStoryScenes));
    }

    [UnityTest]
    public IEnumerator MapScreenSelectsWithoutMutatingStateAndConfirmsOnlyPendingTravel()
    {
        using var harness = new FlowHarness();
        ConfigureMapNode(
            harness.SourceLocation,
            new Vector2(0.2f, 0.5f),
            "출발지",
            MapNodeAccessMode.PersistentUnlock);
        ConfigureMapNode(
            harness.TargetLocation,
            new Vector2(0.58f, 0.51f),
            "목적지",
            MapNodeAccessMode.RouteOnly);
        ConfigureMapNode(
            harness.UnrelatedLocation,
            new Vector2(0.8f, 0.3f),
            "다른 장소",
            MapNodeAccessMode.PersistentUnlock);

        MapDefinition unrelatedMap = harness.Track(
            ScriptableObject.CreateInstance<MapDefinition>());
        SetPrivateField(unrelatedMap, "id", "MAP_OTHER");
        SetPrivateField(unrelatedMap, "displayName", "다른 갑판");
        SetPrivateField(unrelatedMap, "locations", Array.Empty<LocationDefinition>());
        MapDefinition travelMap = harness.Track(
            ScriptableObject.CreateInstance<MapDefinition>());
        SetPrivateField(travelMap, "id", "MAP_TRAVEL");
        SetPrivateField(travelMap, "displayName", "승선 지도");
        SetPrivateField(
            travelMap,
            "locations",
            new[]
            {
                harness.SourceLocation,
                harness.TargetLocation,
                harness.UnrelatedLocation,
            });

        var owner = new GameObject("Map screen under test");
        owner.transform.SetParent(harness.Owner.transform, false);
        owner.SetActive(false);
        MapScreen screen = owner.AddComponent<MapScreen>();
        RectTransform surface = CreateRect("Map Surface", owner.transform);
        Image baseLayer = CreateImage("Base Layer", surface);
        Image restrictedLayer = CreateImage("Restricted Layer", surface);
        Image technicalLayer = CreateImage("Technical Layer", surface);
        RectTransform nodeRoot = CreateRect("Node Root", surface);
        Button nodeTemplate = CreateButton("Location Node Template", nodeRoot);
        Text deckLabel = CreateText("Deck Label", owner.transform);
        Text locationLabel = CreateText("Current Location", owner.transform);
        Text selectionName = CreateText("Selection Name", owner.transform);
        Text selectionStatus = CreateText("Selection Status", owner.transform);
        Text selectionDescription = CreateText("Selection Description", owner.transform);
        Text feedback = CreateText("Travel Feedback", owner.transform);
        Button travelButton = CreateButton("Confirm Travel Button", owner.transform);
        Text travelButtonLabel = travelButton.GetComponentInChildren<Text>(true);
        Toggle restrictedToggle = CreateToggle("Restricted Toggle", owner.transform);
        Toggle technicalToggle = CreateToggle("Technical Toggle", owner.transform);

        SetPrivateField(screen, "state", harness.State);
        SetPrivateField(screen, "flow", harness.Flow);
        SetPrivateField(screen, "maps", new[] { unrelatedMap, travelMap });
        SetPrivateField(screen, "mapSurface", surface);
        SetPrivateField(screen, "baseLayer", baseLayer);
        SetPrivateField(screen, "restrictedLayer", restrictedLayer);
        SetPrivateField(screen, "technicalLayer", technicalLayer);
        SetPrivateField(screen, "nodeRoot", nodeRoot);
        SetPrivateField(screen, "nodeTemplate", nodeTemplate);
        SetPrivateField(screen, "deckLabel", deckLabel);
        SetPrivateField(screen, "locationLabel", locationLabel);
        SetPrivateField(screen, "deckButtons", Array.Empty<Button>());
        SetPrivateField(screen, "restrictedToggle", restrictedToggle);
        SetPrivateField(screen, "technicalToggle", technicalToggle);
        SetPrivateField(screen, "selectionNameLabel", selectionName);
        SetPrivateField(screen, "selectionStatusLabel", selectionStatus);
        SetPrivateField(screen, "selectionDescriptionLabel", selectionDescription);
        SetPrivateField(screen, "feedbackLabel", feedback);
        SetPrivateField(screen, "travelButton", travelButton);
        SetPrivateField(screen, "travelButtonLabel", travelButtonLabel);

        owner.SetActive(true);
        yield return Await(harness.Flow.StartAsync(harness.Source.Id));
        harness.State.UnlockLocation(harness.UnrelatedLocation.Id);
        yield return Await(harness.Flow.AdvanceAsync());
        string sourceSceneId = harness.State.State.currentStorySceneId;
        string sourceLocationId = harness.State.State.currentLocationId;

        yield return Await(screen.OpenAsync(default));

        Assert.That(screen.SelectedMapId, Is.EqualTo(travelMap.Id));
        Assert.That(screen.SelectedLocationId, Is.EqualTo(harness.TargetLocation.Id));
        Assert.That(travelButton.interactable, Is.True);
        Assert.That(restrictedLayer.gameObject.activeSelf, Is.False);
        Assert.That(technicalLayer.gameObject.activeSelf, Is.False);
        Assert.That(restrictedToggle.gameObject.activeSelf, Is.False);
        Assert.That(technicalToggle.gameObject.activeSelf, Is.False);

        Button unrelatedNode = nodeRoot.Find(
            $"LocationNode_{harness.UnrelatedLocation.Id}").GetComponent<Button>();
        unrelatedNode.onClick.Invoke();
        Assert.That(screen.SelectedLocationId, Is.EqualTo(harness.UnrelatedLocation.Id));
        Assert.That(harness.State.State.currentStorySceneId, Is.EqualTo(sourceSceneId));
        Assert.That(harness.State.State.currentLocationId, Is.EqualTo(sourceLocationId));
        Assert.That(travelButton.interactable, Is.False);

        Button targetNode = nodeRoot.Find(
            $"LocationNode_{harness.TargetLocation.Id}").GetComponent<Button>();
        targetNode.onClick.Invoke();
        Assert.That(harness.State.State.currentLocationId, Is.EqualTo(sourceLocationId));
        Assert.That(travelButton.interactable, Is.True);

        travelButton.onClick.Invoke();
        for (var frame = 0;
             frame < 60
                 && !string.Equals(
                     harness.State.State.currentStorySceneId,
                     harness.Target.Id,
                     StringComparison.Ordinal);
             frame++)
        {
            yield return null;
        }

        Assert.That(harness.State.State.currentStorySceneId, Is.EqualTo(harness.Target.Id));
        Assert.That(
            harness.State.State.currentLocationId,
            Is.EqualTo(harness.TargetLocation.Id));
        Assert.That(harness.State.State.pendingStorySceneId, Is.Empty);
    }

    private static void ConfigureMapNode(
        LocationDefinition location,
        Vector2 position,
        string displayName,
        MapNodeAccessMode accessMode)
    {
        SetPrivateField(location, "displayName", displayName);
        var node = new MapNodeDefinition();
        SetPrivateField(node, "id", location.Id);
        SetPrivateField(node, "normalizedPosition", position);
        SetPrivateField(node, "displayName", displayName);
        SetPrivateField(node, "description", $"{displayName} 설명");
        SetPrivateField(node, "accessMode", accessMode);
        SetPrivateField(location, "mapNode", node);
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var owner = new GameObject(name, typeof(RectTransform));
        owner.transform.SetParent(parent, false);
        return owner.GetComponent<RectTransform>();
    }

    private static Image CreateImage(string name, Transform parent)
    {
        RectTransform rect = CreateRect(name, parent);
        return rect.gameObject.AddComponent<Image>();
    }

    private static Text CreateText(string name, Transform parent)
    {
        RectTransform rect = CreateRect(name, parent);
        return rect.gameObject.AddComponent<Text>();
    }

    private static Button CreateButton(string name, Transform parent)
    {
        Image image = CreateImage(name, parent);
        Button button = image.gameObject.AddComponent<Button>();
        Text label = CreateText("Label", image.transform);
        label.text = name;
        return button;
    }

    private static Toggle CreateToggle(string name, Transform parent)
    {
        Image image = CreateImage(name, parent);
        return image.gameObject.AddComponent<Toggle>();
    }

    private static IEnumerator Await(Task task)
    {
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
            throw task.Exception?.InnerException ?? task.Exception;
        if (task.IsCanceled)
            Assert.Fail("The asynchronous flow operation was canceled.");
    }

    private static void SetPrivateField<T>(object target, string name, T value)
    {
        Type type = target.GetType();
        FieldInfo field = null;
        while (type != null && field == null)
        {
            field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            type = type.BaseType;
        }

        Assert.That(field, Is.Not.Null, $"Missing field '{name}'.");
        field.SetValue(target, value);
    }

    private static void InvokePrivate(object target, string name)
    {
        MethodInfo method = target.GetType().GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, $"Missing method '{name}'.");
        method.Invoke(target, null);
    }

    private sealed class FlowHarness : IDisposable
    {
        private readonly List<ScriptableObject> assets = new();

        public FlowHarness(bool addSourceEntryTrustEffect = false)
        {
            Owner = new GameObject("Map travel flow test");
            State = Owner.AddComponent<GameStateStore>();
            LocationPresenter locations = Owner.AddComponent<LocationPresenter>();
            CharacterStage characters = Owner.AddComponent<CharacterStage>();
            Interactions = Owner.AddComponent<InteractionDirector>();
            NarrativeDirector narrative = Owner.AddComponent<NarrativeDirector>();
            AudioDirector audio = Owner.AddComponent<AudioDirector>();
            ScreenRouter screens = Owner.AddComponent<ScreenRouter>();
            TransitionDirector transitions = Owner.AddComponent<TransitionDirector>();
            SequenceDirector sequences = Owner.AddComponent<SequenceDirector>();
            StoryScenes = Owner.AddComponent<StorySceneDirector>();
            Flow = Owner.AddComponent<GameFlowController>();

            var screenOwner = new GameObject("Exploration test screen");
            screenOwner.transform.SetParent(Owner.transform, false);
            ExplorationScreen exploration = screenOwner.AddComponent<ExplorationScreen>();
            SetPrivateField(exploration, "id", ScreenId.Exploration);
            SetPrivateField(screens, "screens", new ScreenBase[] { exploration });
            InvokePrivate(screens, "Awake");

            SetPrivateField(locations, "state", State);
            SetPrivateField(Interactions, "state", State);
            SetPrivateField(StoryScenes, "state", State);
            SetPrivateField(StoryScenes, "locations", locations);
            SetPrivateField(StoryScenes, "characters", characters);
            SetPrivateField(StoryScenes, "interactions", Interactions);
            SetPrivateField(StoryScenes, "narrative", narrative);
            SetPrivateField(StoryScenes, "audioDirector", audio);
            SetPrivateField(StoryScenes, "screens", screens);
            SetPrivateField(StoryScenes, "transitions", transitions);
            SetPrivateField(StoryScenes, "sequences", sequences);

            SourceLocation = CreateLocation("LOC_SOURCE");
            TargetLocation = CreateLocation("LOC_TARGET");
            UnrelatedLocation = CreateLocation("LOC_UNRELATED");
            Source = CreateStoryScene("SOURCE", SourceLocation);
            Target = CreateStoryScene("TARGET", TargetLocation);
            UnrelatedTarget = CreateStoryScene("UNRELATED", UnrelatedLocation);
            var route = new StorySceneRoute();
            SetPrivateField(route, "targetSceneId", Target.Id);
            SetPrivateField(route, "advanceMode", StorySceneAdvanceMode.MapTravel);
            SetPrivateField(Source, "routes", new[] { route });

            if (addSourceEntryTrustEffect)
            {
                ModifyTrustEffect effect = Track(
                    ScriptableObject.CreateInstance<ModifyTrustEffect>());
                SetPrivateField(effect, "characterId", "CHR_DANIEL");
                SetPrivateField(effect, "amount", 1);
                SetPrivateField(Source, "onEnterEffects", new GameEffect[] { effect });
            }

            ContentDatabase content = Track(
                ScriptableObject.CreateInstance<ContentDatabase>());
            SetPrivateField(
                content,
                "storyScenes",
                new[] { Source, Target, UnrelatedTarget });
            SetPrivateField(Flow, "content", content);
            SetPrivateField(Flow, "scenes", StoryScenes);
            SetPrivateField(Flow, "state", State);
            SetPrivateField(Interactions, "flow", Flow);
        }

        public GameObject Owner { get; }
        public GameStateStore State { get; }
        public StorySceneDirector StoryScenes { get; }
        public GameFlowController Flow { get; }
        public InteractionDirector Interactions { get; }
        public StorySceneDefinition Source { get; }
        public StorySceneDefinition Target { get; }
        public StorySceneDefinition UnrelatedTarget { get; }
        public LocationDefinition SourceLocation { get; }
        public LocationDefinition TargetLocation { get; }
        public LocationDefinition UnrelatedLocation { get; }

        public void Dispose()
        {
            foreach (ScriptableObject asset in assets)
                if (asset != null)
                    Object.DestroyImmediate(asset);
            Object.DestroyImmediate(Owner);
        }

        private LocationDefinition CreateLocation(string id)
        {
            LocationDefinition location = Track(
                ScriptableObject.CreateInstance<LocationDefinition>());
            SetPrivateField(location, "id", id);
            return location;
        }

        private StorySceneDefinition CreateStoryScene(
            string id,
            LocationDefinition location)
        {
            StorySceneDefinition scene = Track(
                ScriptableObject.CreateInstance<StorySceneDefinition>());
            SetPrivateField(scene, "id", id);
            SetPrivateField(scene, "location", location);
            SetPrivateField(scene, "initialScreen", ScreenMode.Exploration);
            return scene;
        }

        public T Track<T>(T asset) where T : ScriptableObject
        {
            assets.Add(asset);
            return asset;
        }
    }
}

public sealed class MutatingAdvanceTestAction : InteractionAction
{
    public const string FlagId = "TEST_ADVANCE_EFFECT";

    public override Task<InteractionResult> ExecuteAsync(InteractionContext context)
    {
        context.State.SetFlag(FlagId);
        return Task.FromResult(InteractionResult.CompletedWithStorySceneAdvance);
    }
}

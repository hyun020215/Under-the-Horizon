using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class P01ProgressionTests
{
    private const float TimeoutSeconds = 15f;
    private static readonly SaveSlot Slot3 = new(2);
    private static readonly string[] P01InteractionIds =
    {
        "INT_P_01_INVITATION",
        "INT_P_01_MESSENGER",
        "INT_P_01_DIALOGUE",
        "INT_P_01_CONTINUE",
    };

    private string saveDirectory;
    private SaveService testSaves;

    [SetUp]
    public void SetUp()
    {
        saveDirectory = Path.Combine(
            Path.GetTempPath(),
            "UnderTheHorizonTests",
            "P01Progression",
            Guid.NewGuid().ToString("N"));

        string userSaveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        Assert.That(
            PathsOverlap(saveDirectory, userSaveDirectory),
            Is.False,
            "P-01 progression test saves must not overlap the user save directory.");

        Directory.CreateDirectory(saveDirectory);
        testSaves = new SaveService(saveDirectory);
        AppBootstrap.SaveServiceFactoryOverride = () => testSaves;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        AppBootstrap.SaveServiceFactoryOverride = null;
        try
        {
            Scene cleanup = SceneManager.CreateScene(
                $"P01ProgressionCleanup_{Guid.NewGuid():N}");
            SceneManager.SetActiveScene(cleanup);
            DestroyApplicationRoots();
            yield return null;

            Scene game = SceneManager.GetSceneByName("Game");
            if (game.isLoaded)
                yield return SceneManager.UnloadSceneAsync(game);

            Scene bootstrap = SceneManager.GetSceneByName("Bootstrap");
            if (bootstrap.isLoaded)
                yield return SceneManager.UnloadSceneAsync(bootstrap);
        }
        finally
        {
            AppContext.Services = null;
            testSaves = null;

            if (!string.IsNullOrWhiteSpace(saveDirectory)
                && Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }

            saveDirectory = null;
        }
    }

    [UnityTest]
    public IEnumerator FreshSlotCompletesP01AndRestoresP02AfterRestart()
    {
        yield return LoadGameShell();
        yield return SelectSlot3ThroughUi();

        StorySceneDirector story = Object.FindFirstObjectByType<StorySceneDirector>();
        GameStateStore state = Object.FindFirstObjectByType<GameStateStore>();
        ScreenRouter screens = Object.FindFirstObjectByType<ScreenRouter>();
        NarrativeDirector narrative = Object.FindFirstObjectByType<NarrativeDirector>();
        DialogueScreen dialogue = Object.FindFirstObjectByType<DialogueScreen>(
            FindObjectsInactive.Include);

        Assert.That(story, Is.Not.Null);
        Assert.That(state, Is.Not.Null);
        Assert.That(screens, Is.Not.Null);
        Assert.That(narrative, Is.Not.Null);
        Assert.That(dialogue, Is.Not.Null);

        yield return WaitFor(
            () => story.Current != null && story.Current.Id == "P-01",
            "P-01 did not become the active Story Scene.");
        yield return CompleteCurrentDialogue(dialogue, screens, null, "P-01 opening");

        Assert.That(
            narrative.History.Lines,
            Is.EqualTo(new[] { "P-01_001", "P-01_002" }),
            "P-01 entry must play only the two opening lines before exploration.");
        Assert.That(screens.Current, Is.EqualTo(ScreenId.Exploration));

        CharacterView daniel = null;
        yield return WaitFor(
            () => (daniel = FindActiveCharacter("CHR_DANIEL")) != null,
            "Daniel's CharacterView did not appear in P-01.");
        Assert.That(
            FindActiveInteractionView("INT_P_01_MESSENGER"),
            Is.Null,
            "The messenger affordance must remain hidden before the invitation is inspected.");
        Assert.That(
            daniel.BodyInteractionAvailable,
            Is.False,
            "Daniel's body must not start a different interaction while the invitation is pending.");

        InteractionPointView invitation = null;
        yield return WaitFor(
            () => (invitation = FindActiveHotspot("INT_P_01_INVITATION")) != null,
            "The P-01 invitation hotspot did not appear.");
        Assert.That(invitation.Definition.TargetId, Is.EqualTo("C-01"));
        Assert.That(FindActiveHotspot("INT_P_01_CONTINUE"), Is.Null);
        int historyBeforeInvitation = narrative.History.Lines.Count;
        yield return ClickThroughEventSystem(invitation, "P-01 invitation hotspot");
        yield return CompleteCurrentDialogue(dialogue, screens, null, "invitation inspection");
        yield return WaitFor(
            () => state.IsInteractionCompleted("INT_P_01_INVITATION"),
            "The invitation interaction was not completed.");

        Assert.That(state.HasEvidence("C-01"), Is.True);
        Assert.That(narrative.History.Lines.Count, Is.GreaterThan(historyBeforeInvitation));
        Assert.That(narrative.History.Lines, Does.Contain("P-01_005"));
        Assert.That(FindActiveHotspot("INT_P_01_INVITATION"), Is.Null);
        Assert.That(FindActiveHotspot("INT_P_01_CONTINUE"), Is.Null);

        InteractionPointView messengerBadge = null;
        yield return WaitFor(
            () => (daniel = FindActiveCharacter("CHR_DANIEL")) != null
                && (messengerBadge = daniel.ContextBadge) != null
                && messengerBadge.gameObject.activeInHierarchy
                && messengerBadge.Definition?.Id == "INT_P_01_MESSENGER",
            "Daniel's anchored messenger affordance did not appear after the invitation.");

        Assert.That(messengerBadge.Definition.Type, Is.EqualTo(InteractionType.Context));
        Assert.That(messengerBadge.Definition.TargetId, Is.EqualTo("CHR_DANIEL"));
        Assert.That(
            messengerBadge.transform.IsChildOf(daniel.transform),
            Is.True,
            "The messenger affordance must stay anchored to Daniel's CharacterView.");
        Assert.That(
            daniel.BodyInteractionAvailable,
            Is.False,
            "Daniel's body must not fall back to the pending Context interaction.");
        Assert.That(messengerBadge.Tooltip, Is.Not.Null);
        Assert.That(messengerBadge.Tooltip.IsVisible, Is.False);

        yield return HoverThroughEventSystem(
            messengerBadge,
            "Daniel messenger context affordance");
        Assert.That(messengerBadge.Tooltip.IsVisible, Is.True);
        Assert.That(
            messengerBadge.Tooltip.Text,
            Is.EqualTo(messengerBadge.Definition.DisplayName));

        int historyBeforeMessenger = narrative.History.Lines.Count;
        yield return ClickThroughEventSystem(
            messengerBadge,
            "Daniel messenger context affordance");
        Assert.That(
            messengerBadge.Tooltip.IsVisible,
            Is.False,
            "The Context tooltip must close as soon as its action is selected.");
        yield return CompleteCurrentDialogue(dialogue, screens, null, "messenger inspection");
        yield return WaitFor(
            () => state.IsInteractionCompleted("INT_P_01_MESSENGER")
                && (messengerBadge == null || !messengerBadge.gameObject.activeInHierarchy)
                && (daniel = FindActiveCharacter("CHR_DANIEL")) != null
                && daniel.BodyInteractionAvailable,
            "The messenger affordance did not hand off to Daniel's body interaction.");

        Assert.That(state.HasFlag("anonymous_tip_preview"), Is.True);
        Assert.That(
            narrative.History.Lines.Skip(historyBeforeMessenger),
            Is.EqualTo(new[] { "P-01_006", "P-01_007", "P-01_008" }),
            "The messenger affordance must play only its authored inspection range.");
        Assert.That(narrative.History.Lines, Does.Not.Contain("P-01_009"));
        Assert.That(FindActiveHotspot("INT_P_01_CONTINUE"), Is.Null);

        daniel = FindActiveCharacter("CHR_DANIEL");
        Assert.That(daniel, Is.Not.Null);
        Assert.That(daniel.ContextBadge.gameObject.activeInHierarchy, Is.False);
        int historyBeforeDaniel = narrative.History.Lines.Count;
        int trustAtTabletWarning = int.MinValue;
        Action<DialogueLine> captureTabletWarningTrust = line =>
        {
            if (line.id == "P-01_018")
                trustAtTabletWarning = state.GetTrust("CHR_DANIEL");
        };
        narrative.LineChanged += captureTabletWarningTrust;
        yield return ClickThroughEventSystem(daniel, "Daniel conversation interaction");
        yield return WaitFor(
            () => screens.Current == ScreenId.Dialogue
                && narrative.History.Lines.Count > historyBeforeDaniel,
            "Clicking Daniel's body did not start his conversation.");
        Assert.That(
            narrative.History.Lines[historyBeforeDaniel],
            Is.EqualTo("P-01_009"),
            "Daniel's body interaction must begin at the authored meeting line.");
        yield return CompleteCurrentDialogue(
            dialogue,
            screens,
            "P-01_C1",
            "Daniel conversation and choice");
        narrative.LineChanged -= captureTabletWarningTrust;
        yield return WaitFor(
            () => state.IsInteractionCompleted("INT_P_01_DIALOGUE"),
            "Daniel's P-01 conversation was not completed.");

        Assert.That(state.HasFlag("DIALOGUE_CHOICE_P-01_C1"), Is.True);
        Assert.That(state.HasFlag("daniel_warning_taken"), Is.True);
        Assert.That(state.HasFlag("DIALOGUE_CHOICE_P-01_C2"), Is.False);
        Assert.That(state.GetTrust("CHR_DANIEL"), Is.EqualTo(3));
        string[] danielConversationLines = narrative.History.Lines
            .Skip(historyBeforeDaniel)
            .ToArray();
        Assert.That(
            trustAtTabletWarning,
            Is.EqualTo(2),
            "A fresh save must hear the tablet warning before the choice changes Daniel's trust.");
        Assert.That(danielConversationLines, Does.Contain("P-01_018"));
        Assert.That(danielConversationLines, Does.Contain("P-01_019"));
        Assert.That(
            Array.IndexOf(danielConversationLines, "P-01_018"),
            Is.LessThan(Array.IndexOf(danielConversationLines, "P-01_019")),
            "The tablet warning must precede Daniel's choice prompt.");
        Assert.That(
            danielConversationLines,
            Does.Not.Contain("P-01_006"));
        Assert.That(danielConversationLines, Does.Not.Contain("P-01_007"));
        Assert.That(
            danielConversationLines,
            Does.Not.Contain("P-01_008"),
            "Daniel's conversation must not replay the messenger inspection.");

        InteractionPointView continueHotspot = null;
        yield return WaitFor(
            () => (continueHotspot = FindActiveHotspot("INT_P_01_CONTINUE")) != null,
            "The P-01 continue hotspot did not appear after Daniel's conversation.");
        Assert.That(continueHotspot.Definition.TargetId, Is.EqualTo("LOC_GANGWAY"));
        int historyBeforeP02 = narrative.History.Lines.Count;
        yield return ClickThroughEventSystem(continueHotspot, "P-01 continue hotspot");

        yield return WaitFor(
            () => story.Current != null
                && story.Current.Id == "P-02"
                && state.State.currentStorySceneId == "P-02"
                && testSaves.Exists(Slot3)
                && testSaves.Load(Slot3).currentStorySceneId == "P-02",
            "P-01 did not complete and enter a saved P-02 checkpoint.");
        yield return null;

        AssertP01CompletionState(state.State);
        AssertP02Presentation(story, state);

        GameState saved = testSaves.Load(Slot3);
        AssertP01CompletionState(saved);
        Assert.That(saved.currentStorySceneId, Is.EqualTo("P-02"));
        Assert.That(saved.currentLocationId, Is.EqualTo("LOC_GANGWAY"));

        // Settle P-02's entry dialogue before destroying the running application.
        // Dialogue position is presentation state, so the saved checkpoint remains P-02 entry.
        yield return CompleteCurrentDialogue(dialogue, screens, null, "P-02 entry");
        string[] p02EntryLines = narrative.History.Lines
            .Skip(historyBeforeP02)
            .ToArray();
        Assert.That(p02EntryLines, Does.Contain("P-02_021"));
        Assert.That(
            p02EntryLines,
            Does.Contain("P-02_022"),
            "The serious P-01 choice must unlock Daniel's P-02 trust bonus exchange.");
        yield return WaitFor(
            IsTransitionInputUnblocked,
            "P-02 entry presentation did not release its input blocker.");
        yield return null;

        yield return RestartApplication();
        yield return SelectSlot3ThroughUi();

        StorySceneDirector restoredStory = Object.FindFirstObjectByType<StorySceneDirector>();
        GameStateStore restoredState = Object.FindFirstObjectByType<GameStateStore>();
        NarrativeDirector restoredNarrative = Object.FindFirstObjectByType<NarrativeDirector>();
        ScreenRouter restoredScreens = Object.FindFirstObjectByType<ScreenRouter>();
        DialogueScreen restoredDialogue = Object.FindFirstObjectByType<DialogueScreen>(
            FindObjectsInactive.Include);

        Assert.That(restoredStory, Is.Not.Null);
        Assert.That(restoredState, Is.Not.Null);
        Assert.That(restoredNarrative, Is.Not.Null);
        Assert.That(restoredScreens, Is.Not.Null);
        Assert.That(restoredDialogue, Is.Not.Null);

        yield return WaitFor(
            () => restoredStory.Current != null
                && restoredStory.Current.Id == "P-02"
                && restoredState.State.currentStorySceneId == "P-02"
                && restoredState.State.currentLocationId == "LOC_GANGWAY",
            "Selecting the occupied slot did not restore P-02.");
        yield return WaitFor(
            () => P02CharacterViewsMatch(restoredStory),
            "P-02 character placement was not reconstructed after restart.");

        AssertP01CompletionState(restoredState.State);
        AssertP02Presentation(restoredStory, restoredState);
        Assert.That(
            FindActiveInteractionView("INT_P_01_MESSENGER"),
            Is.Null,
            "Restoring P-02 must not reconstruct P-01's messenger affordance.");
        Assert.That(
            restoredNarrative.History.Lines.Any(id => id.StartsWith("P-01", StringComparison.Ordinal)),
            Is.False,
            "Restoring P-02 must not replay P-01.");

        yield return CompleteCurrentDialogue(
            restoredDialogue,
            restoredScreens,
            null,
            "restored P-02 entry");
        Assert.That(restoredNarrative.History.Lines, Does.Contain("P-02_021"));
        Assert.That(
            restoredNarrative.History.Lines,
            Does.Contain("P-02_022"),
            "Restoring the P-02 checkpoint must preserve the trust bonus eligibility.");
        yield return WaitFor(
            IsTransitionInputUnblocked,
            "Restored P-02 presentation did not release its input blocker.");
    }

    private IEnumerator LoadGameShell()
    {
        yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);
        yield return WaitFor(
            () => SceneManager.GetActiveScene().name == "Game"
                && Object.FindFirstObjectByType<GameFlowController>() != null
                && AppContext.Services != null,
            "Bootstrap did not load the persistent Game shell.");

        Assert.That(
            AppContext.Services.Get<SaveService>(),
            Is.SameAs(testSaves),
            "The application did not use the isolated test SaveService.");
    }

    private IEnumerator RestartApplication()
    {
        Scene restart = SceneManager.CreateScene($"P01Restart_{Guid.NewGuid():N}");
        SceneManager.SetActiveScene(restart);
        DestroyApplicationRoots();
        yield return null;

        AppContext.Services = null;
        AppBootstrap.SaveServiceFactoryOverride = () => testSaves;
        yield return LoadGameShell();
    }

    private static void DestroyApplicationRoots()
    {
        var roots = new HashSet<GameObject>();
        foreach (AppBootstrap bootstrap in Object.FindObjectsByType<AppBootstrap>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            roots.Add(bootstrap.gameObject);
        }

        foreach (AppLifetime lifetime in Object.FindObjectsByType<AppLifetime>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            roots.Add(lifetime.gameObject);
        }

        foreach (GameObject root in roots)
            Object.Destroy(root);
    }

    private static IEnumerator SelectSlot3ThroughUi()
    {
        ScreenRouter screens = null;
        TitleScreen title = null;
        yield return WaitFor(
            () => (screens = Object.FindFirstObjectByType<ScreenRouter>()) != null
                && screens.Current == ScreenId.Title
                && (title = Object.FindFirstObjectByType<TitleScreen>(
                    FindObjectsInactive.Include)) != null
                && title.gameObject.activeInHierarchy,
            "The title screen did not open.");

        Button start = FindButton(title.transform, "StartButton");
        yield return ClickThroughEventSystem(start, "title Start button");

        SaveSlotScreen slots = null;
        yield return WaitFor(
            () => screens.Current == ScreenId.SaveSlot
                && (slots = Object.FindFirstObjectByType<SaveSlotScreen>(
                    FindObjectsInactive.Include)) != null
                && slots.gameObject.activeInHierarchy,
            "The save-slot screen did not open.");

        Button slot3 = FindButton(slots.transform, "Slot3Button");
        yield return ClickThroughEventSystem(slot3, "save Slot 3 button");

        ConfirmDialog confirm = null;
        yield return WaitFor(
            () => (confirm = Object.FindFirstObjectByType<ConfirmDialog>(
                    FindObjectsInactive.Include)) != null
                && confirm.gameObject.activeInHierarchy,
            "The save-slot confirmation dialog did not open.");

        Button confirmButton = FindButton(confirm.transform, "ConfirmButton");
        yield return ClickThroughEventSystem(confirmButton, "save-slot Confirm button");
    }

    private static IEnumerator CompleteCurrentDialogue(
        DialogueScreen dialogue,
        ScreenRouter screens,
        string preferredChoiceId,
        string label)
    {
        yield return WaitFor(
            () => screens.Current == ScreenId.Dialogue
                && dialogue.gameObject.activeInHierarchy,
            $"The {label} dialogue did not open.");

        float deadline = Time.realtimeSinceStartup + TimeoutSeconds;
        while (screens.Current == ScreenId.Dialogue
               && Time.realtimeSinceStartup < deadline)
        {
            Button target = ResolveDialogueButton(dialogue, preferredChoiceId);
            if (target != null)
                yield return ClickThroughEventSystem(
                    target,
                    $"{label} dialogue button",
                    () => screens.Current != ScreenId.Dialogue
                        || !dialogue.gameObject.activeInHierarchy);
            else
                yield return null;
        }

        Assert.That(
            screens.Current,
            Is.Not.EqualTo(ScreenId.Dialogue),
            $"The {label} dialogue did not finish within {TimeoutSeconds} seconds.");
    }

    private static Button ResolveDialogueButton(
        DialogueScreen dialogue,
        string preferredChoiceId)
    {
        Button advance = dialogue.transform.Find("AdvanceButton")?.GetComponent<Button>();
        if (dialogue.IsRevealing)
            return IsUsable(advance) ? advance : null;

        Button[] choices = dialogue.GetComponentsInChildren<Button>(false)
            .Where(button => button.GetComponent<DialogueChoiceBinding>() != null)
            .Where(IsUsable)
            .ToArray();
        if (choices.Length > 0)
        {
            if (!string.IsNullOrWhiteSpace(preferredChoiceId))
            {
                Button preferred = choices.FirstOrDefault(button =>
                    string.Equals(
                        button.GetComponent<DialogueChoiceBinding>().Choice?.Id,
                        preferredChoiceId,
                        StringComparison.Ordinal));
                Assert.That(
                    preferred,
                    Is.Not.Null,
                    $"Choice '{preferredChoiceId}' was not offered by the dialogue UI.");
                return preferred;
            }

            return choices[0];
        }

        return IsUsable(advance) ? advance : null;
    }

    private static bool IsUsable(Button button) =>
        button != null
        && button.gameObject.activeInHierarchy
        && button.interactable
        && button.GetComponentsInParent<CanvasGroup>()
            .All(group => group.alpha > 0.99f && group.interactable && group.blocksRaycasts);

    private static IEnumerator ClickThroughEventSystem(
        Component target,
        string label,
        Func<bool> cancelled = null)
    {
        Assert.That(target, Is.Not.Null, $"Missing click target for {label}.");
        Assert.That(EventSystem.current, Is.Not.Null, $"Missing EventSystem for {label}.");

        float deadline = Time.realtimeSinceStartup + TimeoutSeconds;
        List<RaycastResult> lastHits = null;
        while (Time.realtimeSinceStartup < deadline)
        {
            if (cancelled?.Invoke() == true)
                yield break;

            if (target != null && target.gameObject.activeInHierarchy)
            {
                var pointer = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = ScreenPointAtCenter(target.transform as RectTransform),
                };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                lastHits = hits;

                if (hits.Count > 0 && IsTargetOrChild(hits[0].gameObject, target.gameObject))
                {
                    GameObject receiver = ExecuteEvents.ExecuteHierarchy(
                        hits[0].gameObject,
                        pointer,
                        ExecuteEvents.pointerClickHandler);
                    Assert.That(
                        receiver,
                        Is.Not.Null,
                        $"The top raycast hit for {label} had no pointer-click receiver.");
                    Assert.That(
                        IsTargetOrChild(receiver, target.gameObject)
                            || IsTargetOrChild(target.gameObject, receiver),
                        Is.True,
                        $"The {label} click was handled by an unexpected object '{receiver.name}'.");
                    yield return null;
                    yield break;
                }
            }

            yield return null;
        }

        string hitNames = lastHits == null || lastHits.Count == 0
            ? "none"
            : string.Join(", ", lastHits.Take(5).Select(hit => hit.gameObject.name));
        string transitionStates = string.Join(
            "; ",
            Object.FindObjectsByType<CanvasGroup>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(group => group.name == "TransitionOverlay")
                .Select(group =>
                    $"alpha={group.alpha:F2}, interactable={group.interactable}, "
                    + $"blocksRaycasts={group.blocksRaycasts}, "
                    + $"graphicRaycast={group.GetComponent<Graphic>()?.raycastTarget}, "
                    + $"active={group.gameObject.activeInHierarchy}"));
        Assert.Fail(
            $"Could not raycast and click {label} within {TimeoutSeconds} seconds. "
            + $"Top hits: {hitNames}. Transition overlays: {transitionStates}. "
            + $"App roots: {Object.FindObjectsByType<AppLifetime>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length}.");
    }

    private static IEnumerator HoverThroughEventSystem(Component target, string label)
    {
        Assert.That(target, Is.Not.Null, $"Missing hover target for {label}.");
        Assert.That(EventSystem.current, Is.Not.Null, $"Missing EventSystem for {label}.");

        float deadline = Time.realtimeSinceStartup + TimeoutSeconds;
        List<RaycastResult> lastHits = null;
        while (Time.realtimeSinceStartup < deadline)
        {
            if (target != null && target.gameObject.activeInHierarchy)
            {
                var pointer = new PointerEventData(EventSystem.current)
                {
                    position = ScreenPointAtCenter(target.transform as RectTransform),
                };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                lastHits = hits;

                if (hits.Count > 0 && IsTargetOrChild(hits[0].gameObject, target.gameObject))
                {
                    GameObject receiver = ExecuteEvents.ExecuteHierarchy(
                        hits[0].gameObject,
                        pointer,
                        ExecuteEvents.pointerEnterHandler);
                    Assert.That(
                        receiver,
                        Is.Not.Null,
                        $"The top raycast hit for {label} had no pointer-enter receiver.");
                    Assert.That(
                        IsTargetOrChild(receiver, target.gameObject)
                            || IsTargetOrChild(target.gameObject, receiver),
                        Is.True,
                        $"The {label} hover was handled by an unexpected object '{receiver.name}'.");
                    yield return null;
                    yield break;
                }
            }

            yield return null;
        }

        string hitNames = lastHits == null || lastHits.Count == 0
            ? "none"
            : string.Join(", ", lastHits.Take(5).Select(hit => hit.gameObject.name));
        Assert.Fail(
            $"Could not raycast and hover {label} within {TimeoutSeconds} seconds. "
            + $"Top hits: {hitNames}.");
    }

    private static Vector2 ScreenPointAtCenter(RectTransform rect)
    {
        Assert.That(rect, Is.Not.Null, "Pointer targets must use RectTransform.");
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
        return RectTransformUtility.WorldToScreenPoint(
            camera,
            rect.TransformPoint(rect.rect.center));
    }

    private static bool IsTargetOrChild(GameObject candidate, GameObject target) =>
        candidate == target || candidate.transform.IsChildOf(target.transform);

    private static InteractionPointView FindActiveHotspot(string interactionId) =>
        FindActiveInteractionView(interactionId);

    private static InteractionPointView FindActiveInteractionView(string interactionId) =>
        Object.FindObjectsByType<InteractionPointView>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(view =>
                view.Definition != null
                && string.Equals(view.Definition.Id, interactionId, StringComparison.Ordinal));

    private static CharacterView FindActiveCharacter(string characterId) =>
        Object.FindObjectsByType<CharacterView>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(view =>
                view.Definition != null
                && string.Equals(view.Definition.Id, characterId, StringComparison.Ordinal));

    private static bool IsTransitionInputUnblocked()
    {
        GameObject overlay = GameObject.Find("TransitionOverlay");
        CanvasGroup group = overlay?.GetComponent<CanvasGroup>();
        return group == null || !group.blocksRaycasts;
    }

    private static void AssertP01CompletionState(GameState state)
    {
        Assert.That(state, Is.Not.Null);
        Assert.That(state.completedStoryScenes, Does.Contain("P-01"));
        Assert.That(state.currentStorySceneId, Is.EqualTo("P-02"));
        Assert.That(state.currentLocationId, Is.EqualTo("LOC_GANGWAY"));
        Assert.That(state.discoveredEvidence, Does.Contain("C-01"));
        Assert.That(state.flags, Does.Contain("anonymous_tip_preview"));
        Assert.That(state.flags, Does.Contain("DIALOGUE_CHOICE_P-01_C1"));
        Assert.That(state.flags, Does.Contain("daniel_warning_taken"));
        Assert.That(state.flags, Does.Not.Contain("DIALOGUE_CHOICE_P-01_C2"));
        Assert.That(state.completedInteractions, Is.EquivalentTo(P01InteractionIds));
        Assert.That(state.completedInteractions.Count, Is.EqualTo(4));
        Assert.That(state.trust.TryGetValue("CHR_DANIEL", out int trust), Is.True);
        Assert.That(trust, Is.EqualTo(3));
    }

    private static void AssertP02Presentation(
        StorySceneDirector story,
        GameStateStore state)
    {
        Assert.That(story.Current, Is.Not.Null);
        Assert.That(story.Current.Id, Is.EqualTo("P-02"));
        Assert.That(story.Current.Location, Is.Not.Null);
        Assert.That(story.Current.Location.Id, Is.EqualTo("LOC_GANGWAY"));
        Assert.That(story.Current.CharacterSet, Is.Not.Null);
        Assert.That(story.Current.CharacterSet.name, Is.EqualTo("SET_P_02_CHARACTERS"));
        Assert.That(story.Current.InteractionSet, Is.Not.Null);
        Assert.That(story.Current.InteractionSet.name, Is.EqualTo("INT_P_02"));
        Assert.That(
            Object.FindFirstObjectByType<LocationPresenter>().Current,
            Is.SameAs(story.Current.Location));
        Assert.That(
            Object.FindFirstObjectByType<InteractionDirector>().Current,
            Is.SameAs(story.Current.InteractionSet));
        Assert.That(state.State.currentLocationId, Is.EqualTo("LOC_GANGWAY"));
        Assert.That(P02CharacterViewsMatch(story), Is.True);
    }

    private static bool P02CharacterViewsMatch(StorySceneDirector story)
    {
        CharacterPlacement[] placements = story?.Current?.CharacterSet?.Placements;
        if (placements == null)
            return false;

        string[] expected = placements
            .Where(placement => placement.character != null)
            .Select(placement => placement.character.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        string[] actual = Object.FindObjectsByType<CharacterView>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .Where(view => view.Definition != null)
            .Select(view => view.Definition.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        return actual.SequenceEqual(expected);
    }

    private static Button FindButton(Transform root, string name)
    {
        Button button = root.GetComponentsInChildren<Button>(true)
            .FirstOrDefault(candidate => candidate.name == name);
        Assert.That(button, Is.Not.Null, $"Missing button '{name}'.");
        return button;
    }

    private static IEnumerator WaitFor(
        Func<bool> condition,
        string failureMessage,
        float timeoutSeconds = TimeoutSeconds)
    {
        float deadline = Time.realtimeSinceStartup + timeoutSeconds;
        while (!condition() && Time.realtimeSinceStartup < deadline)
            yield return null;

        Assert.That(condition(), Is.True, failureMessage);
    }

    private static bool PathsOverlap(string first, string second) =>
        IsSameOrChild(first, second) || IsSameOrChild(second, first);

    private static bool IsSameOrChild(string candidate, string parent)
    {
#if UNITY_EDITOR_WIN
        const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
#else
        const StringComparison comparison = StringComparison.Ordinal;
#endif
        string fullCandidate = Path.GetFullPath(candidate)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string fullParent = Path.GetFullPath(parent)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(fullCandidate, fullParent, comparison)
            || fullCandidate.StartsWith(
                fullParent + Path.DirectorySeparatorChar,
                comparison);
    }
}

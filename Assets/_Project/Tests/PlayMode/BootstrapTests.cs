using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class BootstrapTests
{
    private const float SceneLoadTimeoutSeconds = 10f;

    [UnityTest]
    public IEnumerator BootstrapLoadsPersistentGameShell()
    {
        yield return SceneManager.LoadSceneAsync("Bootstrap");

        var elapsed = 0f;
        while (SceneManager.GetActiveScene().name != "Game"
               && elapsed < SceneLoadTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Game"));
        Assert.That(GameObject.Find("GameRoot"), Is.Not.Null);
        Assert.That(Object.FindFirstObjectByType<GameFlowController>(), Is.Not.Null);
        Assert.That(Object.FindFirstObjectByType<AudioListener>(), Is.Not.Null);
        Assert.That(AppContext.Services, Is.Not.Null);
        Assert.That(AppContext.Services.Get<GameDefinition>(), Is.Not.Null);
        Assert.That(AppContext.Services.Get<ContentDatabase>(), Is.Not.Null);
        Assert.That(AppContext.Services.Get<ContentLoader>(), Is.Not.Null);
        Assert.That(AppContext.Services.Get<SaveService>(), Is.Not.Null);
        Assert.That(AppContext.Services.Get<AudioSettingsService>(), Is.Not.Null);

        TitleScreen title = Object.FindFirstObjectByType<TitleScreen>(FindObjectsInactive.Include);
        Assert.That(title, Is.Not.Null);
        ScreenRouter router = Object.FindFirstObjectByType<ScreenRouter>();
        while (router.Current != ScreenId.Title)
            yield return null;
        Assert.That(title.gameObject.activeInHierarchy, Is.True);
        Button start = title.transform.Find("StartButton").GetComponent<Button>();
        start.onClick.Invoke();
        yield return null;

        SaveSlotScreen slots = Object.FindFirstObjectByType<SaveSlotScreen>(
            FindObjectsInactive.Include
        );
        Assert.That(slots, Is.Not.Null);
        while (router.Current != ScreenId.SaveSlot)
            yield return null;
        Assert.That(slots.gameObject.activeInHierarchy, Is.True);
        slots.transform.Find("Slot3Button").GetComponent<Button>().onClick.Invoke();
        yield return null;
        GameObject.Find("ConfirmButton").GetComponent<Button>().onClick.Invoke();
        yield return null;

        NarrativeDirector narrative = Object.FindFirstObjectByType<NarrativeDirector>();
        DialogueScreen dialogue = Object.FindFirstObjectByType<DialogueScreen>(
            FindObjectsInactive.Include
        );
        elapsed = 0f;
        while ((dialogue == null || !dialogue.gameObject.activeInHierarchy
                || narrative.History.Lines.Count < 1)
               && elapsed < SceneLoadTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
            dialogue = Object.FindFirstObjectByType<DialogueScreen>(FindObjectsInactive.Include);
        }

        Assert.That(dialogue, Is.Not.Null);
        Assert.That(dialogue.gameObject.activeInHierarchy, Is.True);
        Assert.That(narrative.History.Lines.Count, Is.EqualTo(1));
        PersistentHud hud = Object.FindFirstObjectByType<PersistentHud>();
        Assert.That(hud, Is.Not.Null);
        Assert.That(hud.gameObject.activeInHierarchy, Is.True);
        Assert.That(
            hud.GetComponentsInParent<CanvasGroup>().All(group => group.alpha > 0.99f),
            Is.True
        );
        Assert.That(
            dialogue.GetComponentsInParent<CanvasGroup>().All(group => group.alpha > 0.99f),
            Is.True
        );
        ExplorationScreen exploration = Object.FindFirstObjectByType<ExplorationScreen>(
            FindObjectsInactive.Include
        );
        Assert.That(exploration.GetComponent<Image>().raycastTarget, Is.False);

        Canvas worldCanvas = GameObject.Find("WorldCanvas").GetComponent<Canvas>();
        Canvas uiCanvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
        Assert.That(uiCanvas.sortingOrder, Is.GreaterThan(worldCanvas.sortingOrder));
        Assert.That(
            uiCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight,
            Is.EqualTo(0.5f).Within(0.001f)
        );
        Assert.That(uiCanvas.GetComponent<CanvasGroup>(), Is.Null);
        AspectRatioFitter backgroundAspect = GameObject.Find("BackgroundLayer")
            .GetComponent<AspectRatioFitter>();
        Assert.That(
            backgroundAspect.aspectMode,
            Is.EqualTo(AspectRatioFitter.AspectMode.EnvelopeParent)
        );
        RectTransform worldFrame = GameObject.Find("WorldFrame")
            .GetComponent<RectTransform>();
        RectTransform uiFrame = GameObject.Find("UIFrame")
            .GetComponent<RectTransform>();
        Assert.That(worldFrame.GetComponent<AspectRatioFitter>(), Is.Null);
        Assert.That(uiFrame.GetComponent<AspectRatioFitter>(), Is.Null);
        Assert.That(worldFrame.anchorMax, Is.EqualTo(Vector2.one));
        Assert.That(uiFrame.anchorMax, Is.EqualTo(Vector2.one));
        Assert.That(
            Object.FindFirstObjectByType<PersistentHud>().transform.parent.name,
            Is.EqualTo("UIFrame")
        );
        Assert.That(
            dialogue.transform.Find("Choice2").GetComponent<DialogueChoiceBinding>(),
            Is.Not.Null
        );

        var reopen = router.OpenAsync(ScreenId.Dialogue);
        while (!reopen.IsCompleted)
            yield return null;
        Assert.That(reopen.IsFaulted, Is.False);
        Assert.That(dialogue.gameObject.activeInHierarchy, Is.True);

        Button advance = dialogue.transform.Find("AdvanceButton").GetComponent<Button>();
        StorySceneDirector story = Object.FindFirstObjectByType<StorySceneDirector>();
        Assert.That(story.Current.EntryDialogue.Lines, Is.Not.Empty);
        elapsed = 0f;
        while (router.Current == ScreenId.Dialogue
               && elapsed < SceneLoadTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            if (dialogue.IsRevealing)
            {
                advance.onClick.Invoke();
            }
            else
            {
                Button choice = dialogue
                    .GetComponentsInChildren<Button>()
                    .FirstOrDefault(button =>
                        button.GetComponent<DialogueChoiceBinding>() != null);
                if (choice != null)
                    choice.onClick.Invoke();
                else
                    advance.onClick.Invoke();
            }

            yield return null;
        }

        Assert.That(router.Current, Is.EqualTo(ScreenId.Exploration));
        Assert.That(narrative.History.Lines.Count, Is.GreaterThan(1));
        CanvasGroup transitionOverlay = GameObject.Find("TransitionOverlay")
            .GetComponent<CanvasGroup>();
        elapsed = 0f;
        while (transitionOverlay.blocksRaycasts && elapsed < SceneLoadTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Assert.That(transitionOverlay.blocksRaycasts, Is.False);

        hud.transform.Find("MapButton").GetComponent<Button>().onClick.Invoke();
        elapsed = 0f;
        while (router.Current != ScreenId.Map && elapsed < SceneLoadTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Assert.That(router.Current, Is.EqualTo(ScreenId.Map));
        MapScreen map = Object.FindFirstObjectByType<MapScreen>(FindObjectsInactive.Include);
        Assert.That(map.gameObject.activeInHierarchy, Is.True);
        Assert.That(GameObject.Find("Base Layer").GetComponent<Image>().sprite, Is.Not.Null);
        map.transform.Find("BackButton").GetComponent<Button>().onClick.Invoke();
        while (router.Current != ScreenId.Exploration)
            yield return null;
        while (transitionOverlay.blocksRaycasts)
            yield return null;

        hud.transform.Find("RecordButton").GetComponent<Button>().onClick.Invoke();
        while (router.Current != ScreenId.InvestigationRecord)
            yield return null;
        InvestigationRecordScreen record = Object.FindFirstObjectByType<InvestigationRecordScreen>(
            FindObjectsInactive.Include);
        Assert.That(record.gameObject.activeInHierarchy, Is.True);
        Assert.That(GameObject.Find("Empty Label"), Is.Not.Null);
        record.transform.Find("BackButton").GetComponent<Button>().onClick.Invoke();
        while (router.Current != ScreenId.Exploration)
            yield return null;
        while (transitionOverlay.blocksRaycasts)
            yield return null;

        int historyBeforeClick = narrative.History.Lines.Count;
        CharacterView character = Object.FindFirstObjectByType<CharacterView>();
        Assert.That(character, Is.Not.Null);
        var pointer = new PointerEventData(EventSystem.current)
        {
            position = RectTransformUtility.WorldToScreenPoint(
                null,
                ((RectTransform)character.transform).TransformPoint(
                    ((RectTransform)character.transform).rect.center
                )
            ),
        };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        Assert.That(hits.Count, Is.GreaterThan(0));
        Assert.That(hits[0].gameObject, Is.EqualTo(character.gameObject));
        ExecuteEvents.Execute(
            character.gameObject,
            pointer,
            ExecuteEvents.pointerClickHandler
        );
        elapsed = 0f;
        while (narrative.History.Lines.Count == historyBeforeClick
               && elapsed < SceneLoadTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Assert.That(narrative.History.Lines.Count, Is.GreaterThan(historyBeforeClick));
    }
}

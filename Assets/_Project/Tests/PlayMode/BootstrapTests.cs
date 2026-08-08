using System.Collections;
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

        TitleScreen title = Object.FindFirstObjectByType<TitleScreen>(FindObjectsInactive.Include);
        Assert.That(title, Is.Not.Null);
        Assert.That(title.gameObject.activeInHierarchy, Is.True);
        Button start = title.transform.Find("StartButton").GetComponent<Button>();
        start.onClick.Invoke();
        yield return null;

        SaveSlotScreen slots = Object.FindFirstObjectByType<SaveSlotScreen>(
            FindObjectsInactive.Include
        );
        Assert.That(slots, Is.Not.Null);
        Assert.That(slots.gameObject.activeInHierarchy, Is.True);
        slots.transform.Find("Slot3Button").GetComponent<Button>().onClick.Invoke();
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
        Assert.That(
            dialogue.transform.Find("Choice2").GetComponent<DialogueChoiceBinding>(),
            Is.Not.Null
        );

        ScreenRouter router = Object.FindFirstObjectByType<ScreenRouter>();
        var reopen = router.OpenAsync(ScreenId.Dialogue);
        while (!reopen.IsCompleted)
            yield return null;
        Assert.That(reopen.IsFaulted, Is.False);
        Assert.That(dialogue.gameObject.activeInHierarchy, Is.True);

        Button advance = dialogue.transform.Find("AdvanceButton").GetComponent<Button>();
        advance.onClick.Invoke();
        yield return null;

        Assert.That(narrative.History.Lines.Count, Is.EqualTo(2));

        StorySceneDirector story = Object.FindFirstObjectByType<StorySceneDirector>();
        int lineCount = story.Current.EntryDialogue.Lines.Length;
        for (var index = 2; index < lineCount; index++)
        {
            advance.onClick.Invoke();
            yield return null;
        }
        advance.onClick.Invoke();
        yield return null;

        int historyBeforeClick = narrative.History.Lines.Count;
        CharacterView character = Object.FindFirstObjectByType<CharacterView>();
        Assert.That(character, Is.Not.Null);
        ExecuteEvents.Execute(
            character.gameObject,
            new PointerEventData(EventSystem.current),
            ExecuteEvents.pointerClickHandler
        );
        yield return null;

        Assert.That(narrative.History.Lines.Count, Is.GreaterThan(historyBeforeClick));
    }
}

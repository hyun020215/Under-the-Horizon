using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class DialogueFlowTests
{
    [UnityTest]
    public IEnumerator NarrativeDirectorPresentsAuthoredLinesInOrder()
    {
        var gameObject = new GameObject("DialogueFlowTest");
        DialogueSequence sequence = ScriptableObject.CreateInstance<DialogueSequence>();

        try
        {
            var lines = new[]
            {
                new DialogueLine { id = "line-1", text = "첫 번째 문장" },
                new DialogueLine { id = "line-2", text = "두 번째 문장" },
            };
            typeof(DialogueSequence)
                .GetField("lines", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(sequence, lines);

            NarrativeDirector director = gameObject.AddComponent<NarrativeDirector>();
            var started = 0;
            var ended = 0;
            director.DialogueStarted += () => started++;
            director.DialogueEnded += () => ended++;
            var task = director.PlayAsync(sequence);
            while (!task.IsCompleted)
                yield return null;

            Assert.That(task.IsFaulted, Is.False);
            Assert.That(director.History.Lines, Is.EqualTo(new[] { "line-1", "line-2" }));
            Assert.That(started, Is.EqualTo(1));
            Assert.That(ended, Is.EqualTo(1));
        }
        finally
        {
            Object.Destroy(sequence);
            Object.Destroy(gameObject);
        }
    }

    [UnityTest]
    public IEnumerator SelectedChoiceJumpsToItsAuthoredNextLine()
    {
        var gameObject = new GameObject("DialogueBranchTest");
        DialogueSequence sequence = ScriptableObject.CreateInstance<DialogueSequence>();

        try
        {
            DialogueChoice choice = CreateChoice("choice-b", "branch-b");
            var lines = new[]
            {
                new DialogueLine
                {
                    id = "choice-owner",
                    text = "선택",
                    choices = new[] { choice },
                },
                new DialogueLine { id = "branch-a", text = "A" },
                new DialogueLine { id = "branch-b", text = "B" },
            };
            typeof(DialogueSequence)
                .GetField("lines", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(sequence, lines);

            NarrativeDirector director = gameObject.AddComponent<NarrativeDirector>();
            director.LinePresented += line =>
                Task.FromResult(line.id == "choice-owner" ? choice : null);
            Task play = director.PlayAsync(sequence);
            while (!play.IsCompleted)
                yield return null;

            Assert.That(play.IsFaulted, Is.False);
            Assert.That(
                director.History.Lines,
                Is.EqualTo(new[] { "choice-owner", "branch-b" }));
        }
        finally
        {
            Object.Destroy(sequence);
            Object.Destroy(gameObject);
        }
    }

    [UnityTest]
    public IEnumerator NarrativeDirectorCanPlayAnAuthoredLineRange()
    {
        var gameObject = new GameObject("DialogueRangeTest");
        DialogueSequence sequence = ScriptableObject.CreateInstance<DialogueSequence>();

        try
        {
            var lines = new[]
            {
                new DialogueLine { id = "line-1", text = "첫 번째" },
                new DialogueLine { id = "line-2", text = "두 번째" },
                new DialogueLine { id = "line-3", text = "세 번째" },
                new DialogueLine { id = "line-4", text = "네 번째" },
            };
            typeof(DialogueSequence)
                .GetField("lines", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(sequence, lines);

            NarrativeDirector director = gameObject.AddComponent<NarrativeDirector>();
            Task play = director.PlayAsync(sequence, "line-2", "line-3");
            while (!play.IsCompleted)
                yield return null;

            Assert.That(play.IsFaulted, Is.False);
            Assert.That(
                director.History.Lines,
                Is.EqualTo(new[] { "line-2", "line-3" }));
        }
        finally
        {
            Object.Destroy(sequence);
            Object.Destroy(gameObject);
        }
    }

    [UnityTest]
    public IEnumerator AudioCrossfadeCreatesAndAlternatesTwoMusicBuses()
    {
        var gameObject = new GameObject("AudioCrossfadeTest");

        try
        {
            gameObject.AddComponent<AudioSource>();
            AudioCrossfade crossfade = gameObject.AddComponent<AudioCrossfade>();
            yield return null;

            Assert.That(gameObject.GetComponents<AudioSource>(), Has.Length.EqualTo(2));
            AudioSource initial = crossfade.ActiveSource;
            crossfade.Play(null, 0.6f, 0f);
            yield return null;

            Assert.That(crossfade.ActiveSource, Is.Not.SameAs(initial));
            Assert.That(crossfade.ActiveSource.volume, Is.EqualTo(0.6f).Within(0.001f));
        }
        finally
        {
            Object.Destroy(gameObject);
        }
    }

    private static DialogueChoice CreateChoice(string id, string nextLineId)
    {
        var choice = new DialogueChoice();
        SetChoiceField(choice, "id", id);
        SetChoiceField(choice, "text", id);
        SetChoiceField(choice, "nextLineId", nextLineId);
        return choice;
    }

    private static void SetChoiceField(
        DialogueChoice choice,
        string name,
        string value)
    {
        typeof(DialogueChoice)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(choice, value);
    }
}

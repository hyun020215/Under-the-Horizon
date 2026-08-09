using System.Collections;
using System.Reflection;
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
            var task = director.PlayAsync(sequence);
            while (!task.IsCompleted)
                yield return null;

            Assert.That(task.IsFaulted, Is.False);
            Assert.That(director.History.Lines, Is.EqualTo(new[] { "line-1", "line-2" }));
        }
        finally
        {
            Object.Destroy(sequence);
            Object.Destroy(gameObject);
        }
    }
}

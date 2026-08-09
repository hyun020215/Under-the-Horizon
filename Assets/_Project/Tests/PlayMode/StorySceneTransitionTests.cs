using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class StorySceneTransitionTests
{
    [UnityTest]
    public IEnumerator TransitionDirectorCompletesProfileWithoutARegisteredPlayer()
    {
        var gameObject = new GameObject("StorySceneTransitionTest");
        TransitionProfile profile = ScriptableObject.CreateInstance<TransitionProfile>();

        try
        {
            TransitionDirector director = gameObject.AddComponent<TransitionDirector>();
            var begin = director.BeginAsync(profile);
            while (!begin.IsCompleted)
                yield return null;
            var end = director.EndAsync(profile);
            while (!end.IsCompleted)
                yield return null;

            Assert.That(begin.IsFaulted, Is.False);
            Assert.That(end.IsFaulted, Is.False);
        }
        finally
        {
            Object.Destroy(profile);
            Object.Destroy(gameObject);
        }
    }
}

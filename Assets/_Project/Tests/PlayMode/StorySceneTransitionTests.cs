using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

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

    [Test]
    public void InputBlockerRemovesTransparentCoverFromGraphicRaycasts()
    {
        var gameObject = new GameObject(
            "TransitionInputBlockerTest",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup));

        try
        {
            CanvasGroup group = gameObject.GetComponent<CanvasGroup>();
            Image cover = gameObject.GetComponent<Image>();
            UIInputBlocker blocker = gameObject.AddComponent<UIInputBlocker>();
            SetPrivateField(blocker, "group", group);

            blocker.SetBlocked(true);
            Assert.That(group.blocksRaycasts, Is.True);
            Assert.That(group.interactable, Is.True);
            Assert.That(cover.raycastTarget, Is.True);

            blocker.SetBlocked(false);
            Assert.That(group.blocksRaycasts, Is.False);
            Assert.That(group.interactable, Is.False);
            Assert.That(cover.raycastTarget, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    [UnityTest]
    public IEnumerator ReducedMotionTransitionStillReleasesInputAfterReveal()
    {
        var gameObject = new GameObject(
            "ReducedMotionTransitionTest",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup));
        TransitionProfile profile = ScriptableObject.CreateInstance<TransitionProfile>();

        try
        {
            CanvasGroup group = gameObject.GetComponent<CanvasGroup>();
            Image cover = gameObject.GetComponent<Image>();
            UIInputBlocker blocker = gameObject.AddComponent<UIInputBlocker>();
            TransitionDirector director = gameObject.AddComponent<TransitionDirector>();
            var accessibility = new AccessibilitySettingsService();

            SetPrivateField(blocker, "group", group);
            SetPrivateField(accessibility, "<ReducedMotion>k__BackingField", true);
            SetPrivateField(director, "accessibility", accessibility);
            SetPrivateField(director, "blocker", blocker);
            blocker.SetBlocked(false);

            var begin = director.BeginAsync(profile);
            while (!begin.IsCompleted)
                yield return null;

            Assert.That(begin.IsFaulted, Is.False);
            Assert.That(group.blocksRaycasts, Is.True);
            Assert.That(cover.raycastTarget, Is.True);

            var end = director.EndAsync(profile);
            while (!end.IsCompleted)
                yield return null;

            Assert.That(end.IsFaulted, Is.False);
            Assert.That(group.blocksRaycasts, Is.False);
            Assert.That(group.interactable, Is.False);
            Assert.That(cover.raycastTarget, Is.False);
        }
        finally
        {
            Object.Destroy(profile);
            Object.Destroy(gameObject);
        }
    }

    private static void SetPrivateField<T>(object target, string name, T value)
    {
        FieldInfo field = target.GetType().GetField(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field '{name}'.");
        field.SetValue(target, value);
    }
}

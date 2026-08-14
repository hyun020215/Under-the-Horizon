using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public sealed class StorySceneRouteTests
{
    [Test]
    public void MissingRouteIsNotAvailable()
    {
        var route = new StorySceneRoute();
        var go = new GameObject();
        try
        {
            Assert.IsFalse(route.IsAvailable(go.AddComponent<GameStateStore>()));
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void ExistingRouteDefaultsToImmediateAdvance()
    {
        var route = new StorySceneRoute();
        SetPrivateField(route, "targetSceneId", "P-02");

        Assert.That(route.AdvanceMode, Is.EqualTo(StorySceneAdvanceMode.Immediate));
    }

    [Test]
    public void ResolveRouteReturnsTheAvailableRouteAndResolveNextRemainsCompatible()
    {
        StorySceneDefinition scene = ScriptableObject.CreateInstance<StorySceneDefinition>();
        var route = new StorySceneRoute();
        var go = new GameObject();
        try
        {
            SetPrivateField(route, "targetSceneId", "P-02");
            SetPrivateField(route, "advanceMode", StorySceneAdvanceMode.MapTravel);
            SetPrivateField(scene, "routes", new[] { route });
            GameStateStore state = go.AddComponent<GameStateStore>();

            Assert.That(scene.ResolveRoute(state), Is.SameAs(route));
            Assert.That(scene.ResolveRoute(state).AdvanceMode, Is.EqualTo(StorySceneAdvanceMode.MapTravel));
            Assert.That(scene.ResolveNext(state), Is.EqualTo("P-02"));
        }
        finally
        {
            Object.DestroyImmediate(scene);
            Object.DestroyImmediate(go);
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

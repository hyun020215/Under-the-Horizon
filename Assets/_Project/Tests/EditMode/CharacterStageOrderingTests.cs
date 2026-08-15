using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class CharacterStageOrderingTests
{
    [Test]
    public void ApplyAsyncOrdersCharactersBySortingOrderAndKeepsShadowsBehindViews()
    {
        GameObject owner = new("Character Stage Test Owner");
        GameObject rootObject = new("Character Root", typeof(RectTransform));
        GameObject prefabObject = new(
            "Character View Prefab",
            typeof(RectTransform),
            typeof(CharacterView));
        CharacterPlacementSet set = ScriptableObject.CreateInstance<CharacterPlacementSet>();
        CharacterDefinition first = ScriptableObject.CreateInstance<CharacterDefinition>();
        CharacterDefinition second = ScriptableObject.CreateInstance<CharacterDefinition>();
        CharacterDefinition third = ScriptableObject.CreateInstance<CharacterDefinition>();

        try
        {
            rootObject.transform.SetParent(owner.transform, false);
            CharacterStage stage = owner.AddComponent<CharacterStage>();
            SetPrivateField(stage, "prefab", prefabObject.GetComponent<CharacterView>());
            SetPrivateField(stage, "root", rootObject.GetComponent<RectTransform>());
            SetPrivateField(set, "placements", new[]
            {
                Placement(first, sortingOrder: 20),
                Placement(second, sortingOrder: 10),
                Placement(third, sortingOrder: 10),
            });

            stage.ApplyAsync(set).GetAwaiter().GetResult();

            CharacterView[] views = rootObject
                .GetComponentsInChildren<CharacterView>(includeInactive: true);
            Assert.That(
                views.Select(view => view.Definition),
                Is.EqualTo(new[] { second, third, first }));

            Transform[] children = rootObject.transform
                .Cast<Transform>()
                .ToArray();
            Assert.That(children, Has.Length.EqualTo(6));
            Assert.That(
                children.Take(3).All(child => child.GetComponent<CharacterView>() == null),
                Is.True,
                "Every ground shadow must render behind every character view.");
            Assert.That(
                children.Skip(3).All(child => child.GetComponent<CharacterView>() != null),
                Is.True,
                "Character views must follow the ordered shadow block.");
        }
        finally
        {
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(prefabObject);
            Object.DestroyImmediate(set);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(third);
        }
    }

    [Test]
    public void BackgroundNormalizedSetUsesDedicatedRootForShadowsAndViews()
    {
        GameObject owner = new("Character Stage Background Test Owner");
        GameObject viewportRoot = new("Viewport Root", typeof(RectTransform));
        GameObject backgroundRoot = new("Background Root", typeof(RectTransform));
        GameObject prefabObject = new(
            "Character View Prefab",
            typeof(RectTransform),
            typeof(CharacterView));
        CharacterPlacementSet set = ScriptableObject.CreateInstance<CharacterPlacementSet>();
        CharacterDefinition character =
            ScriptableObject.CreateInstance<CharacterDefinition>();

        try
        {
            viewportRoot.transform.SetParent(owner.transform, false);
            backgroundRoot.transform.SetParent(owner.transform, false);
            CharacterStage stage = owner.AddComponent<CharacterStage>();
            SetPrivateField(stage, "prefab", prefabObject.GetComponent<CharacterView>());
            SetPrivateField(stage, "root", viewportRoot.GetComponent<RectTransform>());
            SetPrivateField(
                stage,
                "backgroundRoot",
                backgroundRoot.GetComponent<RectTransform>());
            SetPrivateField(
                set,
                "placementSpace",
                CharacterPlacementSpace.BackgroundNormalized);
            SetPrivateField(set, "placements", new[]
            {
                Placement(character, sortingOrder: 0),
            });

            stage.ApplyAsync(set).GetAwaiter().GetResult();

            Assert.That(viewportRoot.transform.childCount, Is.Zero);
            Assert.That(backgroundRoot.transform.childCount, Is.EqualTo(2));
            Assert.That(
                backgroundRoot.transform.GetChild(0).GetComponent<CharacterView>(),
                Is.Null,
                "The ground shadow must share the background-space root.");
            Assert.That(
                backgroundRoot.transform.GetChild(1).GetComponent<CharacterView>(),
                Is.Not.Null,
                "The character view must share the background-space root.");
        }
        finally
        {
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(prefabObject);
            Object.DestroyImmediate(set);
            Object.DestroyImmediate(character);
        }
    }

    [Test]
    public void BackgroundNormalizedSetFailsWhenDedicatedRootIsMissing()
    {
        GameObject owner = new("Character Stage Missing Root Test Owner");
        GameObject prefabObject = new(
            "Character View Prefab",
            typeof(RectTransform),
            typeof(CharacterView));
        CharacterPlacementSet set = ScriptableObject.CreateInstance<CharacterPlacementSet>();
        CharacterDefinition character =
            ScriptableObject.CreateInstance<CharacterDefinition>();

        try
        {
            CharacterStage stage = owner.AddComponent<CharacterStage>();
            SetPrivateField(stage, "prefab", prefabObject.GetComponent<CharacterView>());
            SetPrivateField(
                set,
                "placementSpace",
                CharacterPlacementSpace.BackgroundNormalized);
            SetPrivateField(set, "placements", new[]
            {
                Placement(character, sortingOrder: 0),
            });

            Assert.Throws<System.InvalidOperationException>(() =>
                stage.ApplyAsync(set).GetAwaiter().GetResult());
        }
        finally
        {
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(prefabObject);
            Object.DestroyImmediate(set);
            Object.DestroyImmediate(character);
        }
    }

    private static CharacterPlacement Placement(
        CharacterDefinition character,
        int sortingOrder) => new()
    {
        character = character,
        normalizedX = 0.5f,
        normalizedY = 0.04f,
        scale = 1f,
        sortingOrder = sortingOrder,
        clickable = true,
    };

    private static void SetPrivateField(object target, string name, object value) =>
        target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(target, value);
}

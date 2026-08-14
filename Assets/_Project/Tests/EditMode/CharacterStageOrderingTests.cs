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

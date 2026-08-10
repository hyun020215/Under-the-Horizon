using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public sealed class SaveSlotPresentationTests
{
    [Test]
    public void SaveScreenUsesThreeHorizontalProgressCards()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(
            "Assets/_Project/Prefabs/UI/PF_SaveSlotScreen.prefab");
        try
        {
            SaveSlotScreen screen = root.GetComponent<SaveSlotScreen>();
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            Assert.That(buttons.Length, Is.EqualTo(3));
            Assert.That(buttons[0].GetComponent<RectTransform>().anchorMin.y,
                Is.EqualTo(buttons[1].GetComponent<RectTransform>().anchorMin.y));
            Assert.That(root.GetComponentsInChildren<Text>(true),
                Has.Some.Matches<Text>(text => text.name == "Chapter"));
            Assert.That(screen, Is.Not.Null);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}

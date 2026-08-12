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
            Button[] slots = System.Array.FindAll(buttons,
                button => button.name.StartsWith("Slot"));
            Button[] deletes = System.Array.FindAll(buttons,
                button => button.name.StartsWith("DeleteSlot"));
            Assert.That(slots.Length, Is.EqualTo(3));
            Assert.That(deletes.Length, Is.EqualTo(3));
            Assert.That(slots[0].GetComponent<RectTransform>().anchorMin.y,
                Is.EqualTo(slots[1].GetComponent<RectTransform>().anchorMin.y));
            foreach (Button slot in slots)
            {
                RectTransform rect = slot.GetComponent<RectTransform>();
                Assert.That(rect.anchorMax.y - rect.anchorMin.y,
                    Is.GreaterThanOrEqualTo(.45f));
                Assert.That(rect.anchorMax.x - rect.anchorMin.x,
                    Is.GreaterThanOrEqualTo(.28f));
            }
            Text title = System.Array.Find(root.GetComponentsInChildren<Text>(true),
                text => text.name == "Save Slot Title");
            Assert.That(title, Is.Not.Null);
            Assert.That(title.fontSize, Is.GreaterThanOrEqualTo(48));
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

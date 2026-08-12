using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class ConfirmDialogPresentationTests
{
    [Test]
    public void ConfirmDialogUsesFullScreenDimAndWideCentralChoiceArea()
    {
        Scene scene = EditorSceneManager.OpenScene(
            "Assets/_Project/Scenes/Game.unity", OpenSceneMode.Additive);
        try
        {
            Transform[] transforms = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .ToArray();
            RectTransform modal = transforms.Single(item => item.name == "ConfirmModal")
                .GetComponent<RectTransform>();
            RectTransform panel = transforms.Single(item => item.name == "Confirm Panel")
                .GetComponent<RectTransform>();
            Text message = panel.Find("Message").GetComponent<Text>();
            RectTransform cancel = panel.Find("CancelButton").GetComponent<RectTransform>();
            RectTransform confirm = panel.Find("ConfirmButton").GetComponent<RectTransform>();

            Assert.That(modal.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(modal.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(modal.GetComponent<Image>().color.a, Is.GreaterThanOrEqualTo(0.9f));
            Assert.That(panel.anchorMax.x - panel.anchorMin.x, Is.GreaterThanOrEqualTo(0.6f));
            Assert.That(panel.anchorMax.y - panel.anchorMin.y, Is.GreaterThanOrEqualTo(0.5f));
            Assert.That(message.fontSize, Is.GreaterThanOrEqualTo(36));
            Assert.That(cancel.anchorMax.y - cancel.anchorMin.y, Is.GreaterThanOrEqualTo(0.2f));
            Assert.That(confirm.anchorMax.y - confirm.anchorMin.y, Is.GreaterThanOrEqualTo(0.2f));
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}

using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public sealed class DialoguePresentationTests
{
    [Test]
    public void DialogueScreenHasExplicitNarrationModeAndHidesInternalIds()
    {
        string source = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Screens/DialogueScreen.cs");
        Assert.That(source, Does.Contain("line.speaker == null"));
        Assert.That(source, Does.Contain("\"NARRATION\""));
        Assert.That(source, Does.Contain("TextAnchor.UpperCenter"));
        Assert.That(source, Does.Not.Contain("sceneLabel.text = sequence.Id"));
    }

    [Test]
    public void DialoguePrefabProvidesASeparateSpeakerPortrait()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(
            "Assets/_Project/Prefabs/UI/PF_DialogueScreen.prefab");
        try
        {
            Transform portrait = root.transform.Find("Speaker Portrait");
            Assert.That(portrait, Is.Not.Null);
            Assert.That(portrait.GetComponent<Image>(), Is.Not.Null);
            Assert.That(portrait.GetComponent<CanvasGroup>(), Is.Not.Null);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [Test]
    public void DialogueTypographyScalesDownForLongLines()
    {
        int shortLine = DialogueTypography.ResolveFontSize(34, 30, false);
        int mediumLine = DialogueTypography.ResolveFontSize(34, 110, false);
        int longLine = DialogueTypography.ResolveFontSize(34, 220, false);
        Assert.That(shortLine, Is.GreaterThan(mediumLine));
        Assert.That(mediumLine, Is.GreaterThan(longLine));
        Assert.That(longLine, Is.GreaterThanOrEqualTo(28));
        Assert.That(DialogueTypography.ResolveFontSize(34, 30, true),
            Is.LessThan(shortLine));
    }

    [Test]
    public void DialoguePrefabKeepsReadableFullHdBodyType()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(
            "Assets/_Project/Prefabs/UI/PF_DialogueScreen.prefab");
        try
        {
            Text body = root.transform.Find("Dialogue Panel/BodyLabel")?.GetComponent<Text>();
            Assert.That(body, Is.Not.Null);
            Assert.That(body.fontSize, Is.GreaterThanOrEqualTo(34));
            RectTransform panel = root.transform.Find("Dialogue Panel") as RectTransform;
            Assert.That(panel.anchorMax.y - panel.anchorMin.y, Is.GreaterThanOrEqualTo(.36f));
            Assert.That(panel.anchorMax.x - panel.anchorMin.x, Is.InRange(.55f, .65f));
            RectTransform bodyRect = body.rectTransform;
            Assert.That(bodyRect.anchorMin.x, Is.GreaterThanOrEqualTo(.08f));
            Assert.That(bodyRect.anchorMax.x, Is.LessThanOrEqualTo(.92f));
            RectTransform nameplate = root.transform.Find("Speaker Nameplate") as RectTransform;
            RectTransform advance = root.transform.Find("AdvanceButton") as RectTransform;
            Assert.That(nameplate.anchorMax.y, Is.LessThanOrEqualTo(panel.anchorMax.y));
            Assert.That(advance.anchorMin.x, Is.GreaterThan(panel.anchorMin.x));
            Assert.That(advance.anchorMax.x, Is.LessThanOrEqualTo(panel.anchorMax.x));
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [Test]
    public void CharacterStageAppliesAuthoredLineExpressionToTheSpeaker()
    {
        string stage = File.ReadAllText(
            "Assets/_Project/Runtime/Characters/CharacterStage.cs");
        Assert.That(stage, Does.Contain("view.ApplyExpression(line.expression)"));
    }
}

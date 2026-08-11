using System.IO;
using NUnit.Framework;

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
    public void DialogueTypographyScalesDownForLongLines()
    {
        int shortLine = DialogueTypography.ResolveFontSize(26, 30, false);
        int mediumLine = DialogueTypography.ResolveFontSize(26, 110, false);
        int longLine = DialogueTypography.ResolveFontSize(26, 220, false);
        Assert.That(shortLine, Is.GreaterThan(mediumLine));
        Assert.That(mediumLine, Is.GreaterThan(longLine));
        Assert.That(DialogueTypography.ResolveFontSize(26, 30, true),
            Is.LessThan(shortLine));
    }

    [Test]
    public void CharacterStageAppliesAuthoredLineExpressionToTheSpeaker()
    {
        string stage = File.ReadAllText(
            "Assets/_Project/Runtime/Characters/CharacterStage.cs");
        Assert.That(stage, Does.Contain("view.ApplyExpression(line.expression)"));
    }
}

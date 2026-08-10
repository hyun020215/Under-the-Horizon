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
}

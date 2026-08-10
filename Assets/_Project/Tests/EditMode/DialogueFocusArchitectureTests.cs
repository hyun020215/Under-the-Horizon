using System.IO;
using NUnit.Framework;

public sealed class DialogueFocusArchitectureTests
{
    [Test]
    public void CharacterStageConsumesNarrativeNotificationWithoutUiWorldDependency()
    {
        string narrative = File.ReadAllText(
            "Assets/_Project/Runtime/Narrative/NarrativeDirector.cs");
        string stage = File.ReadAllText(
            "Assets/_Project/Runtime/Characters/CharacterStage.cs");
        string dialogue = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Screens/DialogueScreen.cs");
        Assert.That(narrative, Does.Contain("LineChanged"));
        Assert.That(stage, Does.Contain("narrative.LineChanged +="));
        Assert.That(dialogue, Does.Not.Contain("CharacterStage"));
    }

    [Test]
    public void FocusTuningComesFromCharacterPresentationProfile()
    {
        string profile = File.ReadAllText(
            "Assets/_Project/Runtime/Characters/CharacterPresentationProfile.cs");
        string view = File.ReadAllText(
            "Assets/_Project/Runtime/Characters/CharacterView.cs");
        Assert.That(profile, Does.Contain("dialogueFocusTint"));
        Assert.That(profile, Does.Contain("dialogueUnfocusedTint"));
        Assert.That(view, Does.Contain("DialogueFocusTint"));
        Assert.That(view, Does.Contain("DialogueUnfocusedTint"));
    }
}

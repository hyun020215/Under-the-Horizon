using System.IO;
using NUnit.Framework;
using UnityEditor;

public sealed class ResponsiveUiCaptureTests
{
    private static readonly string[] RequiredScreens =
    {
        "PF_TitleScreen", "PF_SaveSlotScreen", "PF_ExplorationScreen",
        "PF_DialogueScreen", "PF_MapScreen", "PF_InvestigationScreen",
        "PF_RecordScreen", "PF_InterrogationScreen", "PF_EvidenceBoardScreen",
        "PF_ReconstructionScreen", "PF_PuzzleScreen", "PF_EndingScreen",
        "PF_CreditsScreen", "PF_SettingsScreen",
    };

    [Test]
    public void EveryRoutedScreenHasACaptureTarget()
    {
        string source = File.ReadAllText(
            "Assets/_Project/Editor/Preview/ResponsiveUiCaptureRunner.cs");
        foreach (string screen in RequiredScreens)
        {
            Assert.That(source, Does.Contain($"\"{screen}\""),
                $"Responsive capture coverage is missing {screen}.");
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(
                $"Assets/_Project/Prefabs/UI/{screen}.prefab"), Is.Not.Null);
        }
    }

    [TestCase(1280, 720)]
    [TestCase(1920, 1080)]
    [TestCase(2560, 1440)]
    public void CaptureResolutionsRemainSixteenByNine(int width, int height)
    {
        Assert.That(width / (float)height, Is.EqualTo(16f / 9f).Within(0.001f));
        string source = File.ReadAllText(
            "Assets/_Project/Editor/Preview/ResponsiveUiCaptureRunner.cs");
        Assert.That(source, Does.Contain($"({width}, {height})"));
    }
}

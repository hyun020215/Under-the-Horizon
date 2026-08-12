using NUnit.Framework;
using System.IO;

public sealed class DisplaySettingsTests
{
    [Test]
    public void RecommendedResolutionMatchesLegacySixteenByNineBaseline()
    {
        Assert.That(DisplaySettingsService.RecommendedWidth, Is.EqualTo(1920));
        Assert.That(DisplaySettingsService.RecommendedHeight, Is.EqualTo(1080));
        Assert.That(
            DisplaySettingsService.RecommendedWidth /
            (float)DisplaySettingsService.RecommendedHeight,
            Is.EqualTo(16f / 9f).Within(0.001f));
    }

    [TestCase(1920, 1080, 2)]
    [TestCase(1366, 768, 0)]
    [TestCase(3440, 1440, 4)]
    public void FindsClosestSupportedResolution(int width, int height, int expected)
    {
        var settings = new DisplaySettingsService();
        Assert.That(settings.FindClosestIndex(width, height), Is.EqualTo(expected));
    }

    [Test]
    public void EverySupportedResolutionUsesSixteenByNine()
    {
        var settings = new DisplaySettingsService();
        foreach (DisplaySettingsService.DisplayResolution resolution in settings.Resolutions)
            Assert.That(resolution.Width / (float)resolution.Height,
                Is.EqualTo(16f / 9f).Within(0.001f));
    }

    [Test]
    public void ShippingCanvasesShareTheRecommendedReferenceResolution()
    {
        foreach (string scene in new[]
                 {
                     "Assets/_Project/Scenes/Bootstrap.unity",
                     "Assets/_Project/Scenes/Game.unity",
                 })
        {
            string yaml = File.ReadAllText(scene);
            Assert.That(yaml, Does.Contain("m_ReferenceResolution: {x: 1920, y: 1080}"), scene);
            Assert.That(yaml, Does.Contain("m_MatchWidthOrHeight: 0.5"), scene);
        }
    }
}

using NUnit.Framework;

public sealed class RenderingSettingsTests
{
    [Test]
    public void CanonicalRenderingSettingsPassBuildPreflightRules()
    {
        var errors = RenderingSettingsValidator.ValidateAll();
        Assert.That(errors, Is.Empty, string.Join("\n", errors));
    }
}

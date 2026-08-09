using NUnit.Framework;

public sealed class ContentValidationTests
{
    [Test]
    public void AllAuthoredContentPassesBuildPreflightRules()
    {
        var errors = ContentValidator.ValidateAll();
        Assert.That(errors, Is.Empty, string.Join("\n", errors));
    }
}

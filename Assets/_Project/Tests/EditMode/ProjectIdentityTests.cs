using NUnit.Framework;

public sealed class ProjectIdentityTests
{
    [Test]
    public void UnityPlayerSettingsUseCanonicalProjectIdentity()
    {
        var errors = ProjectIdentityValidator.ValidateAll();
        Assert.That(errors, Is.Empty, string.Join("\n", errors));
    }
}

using System.IO;
using NUnit.Framework;

public sealed class PresentationProfileArchitectureTests
{
    [Test]
    public void LocationAndCharacterPresentationValuesComeFromProfiles()
    {
        string location = File.ReadAllText(
            "Assets/_Project/Runtime/Locations/LocationPresenter.cs");
        string character = File.ReadAllText(
            "Assets/_Project/Runtime/Characters/CharacterIdleMotion.cs");
        Assert.That(location, Does.Contain("AmbientParticleProfile"));
        Assert.That(location, Does.Not.Contain("Color.Lerp"));
        Assert.That(character, Does.Contain("CharacterPresentationProfile"));
        Assert.That(character, Does.Not.Contain("GetHashCode"));
    }

    [Test]
    public void PresentationSchemaChangesAreAdditiveOverrides()
    {
        string locationState = File.ReadAllText(
            "Assets/_Project/Runtime/Locations/LocationStateDefinition.cs");
        string character = File.ReadAllText(
            "Assets/_Project/Runtime/Characters/CharacterDefinition.cs");
        Assert.That(locationState, Does.Contain("ambientParticles"));
        Assert.That(character, Does.Contain("presentationOverride"));
    }
}

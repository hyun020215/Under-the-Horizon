using NUnit.Framework;
using UnityEditor;

public sealed class TitleAtmospherePresentationTests
{
    [Test]
    public void TitleAtmosphereProfileReferencesLayeredLightAndWaterSettings()
    {
        AmbientParticleProfile profile = AssetDatabase.LoadAssetAtPath<AmbientParticleProfile>(
            "Assets/_Project/Content/UI/UI_AMBIENCE_TITLE.asset");
        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.LightShaftSprite, Is.Not.Null);
        Assert.That(profile.LightShaftMaterial, Is.Not.Null);
        Assert.That(profile.LightShaftOpacity, Is.GreaterThan(0f));
        Assert.That(profile.WaterShimmerOpacity, Is.GreaterThan(0f));
    }

    [Test]
    public void LocationAtmosphereKeepsOptionalLayersDisabled()
    {
        AmbientParticleProfile profile = AssetDatabase.LoadAssetAtPath<AmbientParticleProfile>(
            "Assets/_Project/Content/UI/UI_AMBIENCE_LOCATION.asset");
        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.LightShaftOpacity, Is.Zero);
        Assert.That(profile.WaterShimmerOpacity, Is.Zero);
    }
}

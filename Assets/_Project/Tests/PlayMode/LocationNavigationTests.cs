using NUnit.Framework;
using UnityEngine;

public sealed class LocationNavigationTests
{
    [Test]
    public void SettingCurrentLocationAlsoUnlocksIt()
    {
        var gameObject = new GameObject("LocationNavigationTest");
        try
        {
            GameStateStore state = gameObject.AddComponent<GameStateStore>();
            state.SetCurrentLocation("LOC_HORIZON");

            Assert.That(state.State.currentLocationId, Is.EqualTo("LOC_HORIZON"));
            Assert.That(state.State.unlockedLocations, Does.Contain("LOC_HORIZON"));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}

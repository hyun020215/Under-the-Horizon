using System;
using NUnit.Framework;
using UnityEngine;

public sealed class SaveMigrationTests
{
    [Test]
    public void RoundTripPreservesLogicalState()
    {
        var state = new GameState();
        state.currentStorySceneId = "D1-06";
        state.flags.Add("found_body");
        state.trust["RICHARD"] = 7;
        GameState loaded = SaveSerializer.Deserialize(SaveSerializer.Serialize(state));
        Assert.AreEqual("D1-06", loaded.currentStorySceneId);
        Assert.IsTrue(loaded.flags.Contains("found_body"));
        Assert.AreEqual(7, loaded.trust["RICHARD"]);
    }

    [Test]
    public void VersionTwoRoundTripPreservesPendingStorySceneTravel()
    {
        var state = new GameState
        {
            currentStorySceneId = "P-01",
            currentLocationId = "LOC_PORT",
            pendingStorySceneId = "P-02",
        };
        state.completedStoryScenes.Add("P-01");

        string json = SaveSerializer.Serialize(state);
        GameState loaded = SaveSerializer.Deserialize(json);

        Assert.That(json, Does.Contain("\"version\": 2"));
        Assert.That(loaded.currentStorySceneId, Is.EqualTo("P-01"));
        Assert.That(loaded.currentLocationId, Is.EqualTo("LOC_PORT"));
        Assert.That(loaded.pendingStorySceneId, Is.EqualTo("P-02"));
        Assert.That(loaded.completedStoryScenes, Does.Contain("P-01"));
    }

    [Test]
    public void RawVersionOneJsonMigratesWithNoPendingTravel()
    {
        const string json = "{"
            + "\"version\":1,"
            + "\"currentStorySceneId\":\"P-01\","
            + "\"currentLocationId\":\"LOC_PORT\","
            + "\"completedStoryScenes\":[\"P-01\"]"
            + "}";

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        SaveData migrated = SaveMigrationRegistry.Migrate(data);
        GameState loaded = migrated.ToState();

        Assert.That(migrated.version, Is.EqualTo(2));
        Assert.That(migrated.pendingStorySceneId, Is.Empty);
        Assert.That(loaded.currentStorySceneId, Is.EqualTo("P-01"));
        Assert.That(loaded.currentLocationId, Is.EqualTo("LOC_PORT"));
        Assert.That(loaded.completedStoryScenes, Does.Contain("P-01"));
        Assert.That(loaded.pendingStorySceneId, Is.Empty);
    }

    [Test]
    public void SaveFromANewerVersionIsRejected()
    {
        const string json = "{\"version\":3,\"currentStorySceneId\":\"P-01\"}";

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => SaveSerializer.Deserialize(json));

        Assert.That(exception.Message, Does.Contain("newer than supported"));
    }
}

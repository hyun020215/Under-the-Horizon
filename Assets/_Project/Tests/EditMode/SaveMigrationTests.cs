using NUnit.Framework;

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
}

using NUnit.Framework;

public sealed class EndingFlowTests
{
    [Test]
    public void EndingIdSurvivesSerialization()
    {
        var state = new GameState { endingId = "ENDING_A" };
        GameState restored = SaveSerializer.Deserialize(SaveSerializer.Serialize(state));

        Assert.That(restored.endingId, Is.EqualTo("ENDING_A"));
    }
}

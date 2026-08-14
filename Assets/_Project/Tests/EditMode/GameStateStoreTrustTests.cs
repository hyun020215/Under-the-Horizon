using NUnit.Framework;
using UnityEngine;

public sealed class GameStateStoreTrustTests
{
    private GameObject owner;
    private GameStateStore state;

    [SetUp]
    public void SetUp()
    {
        owner = new GameObject("GameStateStoreTrustTests");
        state = owner.AddComponent<GameStateStore>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(owner);
    }

    [Test]
    public void MissingTrustUsesDefaultAndModificationStartsFromIt()
    {
        Assert.That(state.GetTrust("CHR_DANIEL"), Is.EqualTo(GameStateStore.DefaultTrust));

        state.ModifyTrust("CHR_DANIEL", 1);

        Assert.That(state.GetTrust("CHR_DANIEL"), Is.EqualTo(3));
    }

    [Test]
    public void ExplicitStoredZeroTrustRemainsZero()
    {
        var replacement = new GameState();
        replacement.trust["CHR_DANIEL"] = 0;
        state.Replace(replacement);

        Assert.That(state.GetTrust("CHR_DANIEL"), Is.Zero);

        state.ModifyTrust("CHR_DANIEL", 1);

        Assert.That(state.GetTrust("CHR_DANIEL"), Is.EqualTo(1));
    }
}

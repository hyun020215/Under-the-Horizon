using NUnit.Framework;
using UnityEngine;

public sealed class GameEffectTests
{
    [Test]
    public void StateStoreClampsMetrics()
    {
        var go = new GameObject();
        try
        {
            var store = go.AddComponent<GameStateStore>();
            store.ChangeAnxiety(200);
            store.ChangeIntegrity(-200);
            Assert.AreEqual(100, store.State.publicAnxiety);
            Assert.AreEqual(0, store.State.evidenceIntegrity);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}

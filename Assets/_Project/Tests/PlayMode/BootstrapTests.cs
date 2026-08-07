using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class BootstrapTests
{
    private const float SceneLoadTimeoutSeconds = 10f;

    [UnityTest]
    public IEnumerator BootstrapLoadsPersistentGameShell()
    {
        yield return SceneManager.LoadSceneAsync("Bootstrap");

        var elapsed = 0f;
        while (SceneManager.GetActiveScene().name != "Game"
               && elapsed < SceneLoadTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Game"));
        Assert.That(GameObject.Find("GameRoot"), Is.Not.Null);
        Assert.That(Object.FindFirstObjectByType<GameFlowController>(), Is.Not.Null);
    }
}

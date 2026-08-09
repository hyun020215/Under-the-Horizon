using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class CinematicOverlayTests
{
    [UnityTest]
    public IEnumerator FourFrameMontageBlocksWithOverlayAndCleansUp()
    {
        var host = new GameObject("CinematicOverlayTest");
        CinematicOverlayPresenter presenter =
            host.AddComponent<CinematicOverlayPresenter>();
        var frames = new[]
        {
            NewFrame(Color.red),
            NewFrame(Color.green),
            NewFrame(Color.blue),
            NewFrame(Color.white)
        };

        try
        {
            var task = presenter.PlayAsync(
                frames,
                new[] { 0.02f, 0.02f, 0.02f, 0.02f },
                0.01f,
                0.01f,
                0.01f,
                1.035f);

            Assert.That(presenter.IsPlaying, Is.True);
            while (!task.IsCompleted)
                yield return null;

            Assert.That(task.IsFaulted, Is.False);
            Assert.That(presenter.IsPlaying, Is.False);
            Assert.That(presenter.CurrentFrame, Is.Null);
        }
        finally
        {
            foreach (Texture2D frame in frames)
                Object.Destroy(frame);
            Object.Destroy(host);
        }
    }

    private static Texture2D NewFrame(Color color)
    {
        var texture = new Texture2D(2, 2);
        texture.SetPixels(new[] { color, color, color, color });
        texture.Apply();
        return texture;
    }
}

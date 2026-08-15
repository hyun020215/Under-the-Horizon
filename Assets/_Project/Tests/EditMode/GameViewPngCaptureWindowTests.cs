using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class GameViewPngCaptureWindowTests
{
    [Test]
    public void SupportedResolutionsMatchTheCompositeApprovalMatrix()
    {
        Assert.That(
            GameViewPngCaptureWindow.SupportedResolutions,
            Is.EqualTo(new[]
            {
                new Vector2Int(1920, 1080),
                new Vector2Int(2560, 1440),
                new Vector2Int(1920, 1200),
                new Vector2Int(2560, 1080),
                new Vector2Int(3440, 1440),
            }));
    }

    [Test]
    public void BuildOutputPathUsesIgnoredValidationFolderAndStableName()
    {
        string path = GameViewPngCaptureWindow.BuildOutputPath(
            "C:/Project",
            "2026-08-15_7b5aaebe",
            "p01-invitation-visual-approval",
            "before hover",
            1920,
            1080);

        Assert.That(
            path.Replace('\\', '/'),
            Is.EqualTo(
                "C:/Project/Logs/Validation/"
                + "2026-08-15_7b5aaebe_p01-invitation-visual-approval/"
                + "1920x1080_p01-invitation-visual-approval_before-hover.png"));
    }

    [TestCase("../P-01\\idle", "P-01-idle")]
    [TestCase("  focus state  ", "focus-state")]
    [TestCase("///", "fallback")]
    public void SanitizePathSegmentPreventsPathTraversal(
        string value,
        string expected)
    {
        Assert.That(
            GameViewPngCaptureWindow.SanitizePathSegment(value, "fallback"),
            Is.EqualTo(expected));
    }

    [Test]
    public void BuildOutputPathRejectsInvalidDimensions()
    {
        Assert.That(
            () => GameViewPngCaptureWindow.BuildOutputPath(
                Path.GetTempPath(), "session", "scope", "idle", 0, 1080),
            Throws.TypeOf<System.ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetAvailableOutputPathPreservesExistingEvidence()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "UnderTheHorizonTests",
            System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string requested = Path.Combine(directory, "1920x1080_scope_idle.png");
            string second = Path.Combine(directory, "1920x1080_scope_idle-02.png");
            File.WriteAllText(requested, "existing");
            File.WriteAllText(second, "existing");

            Assert.That(
                GameViewPngCaptureWindow.GetAvailableOutputPath(requested),
                Is.EqualTo(Path.Combine(directory, "1920x1080_scope_idle-03.png")));
            Assert.That(File.ReadAllText(requested), Is.EqualTo("existing"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public void StablePlayFramesDoNotAdvanceTwiceInTheSameFrame()
    {
        int lastObservedFrame = -1;
        int stableFrames = 0;

        stableFrames = GameViewPngCaptureWindow.AdvanceStablePlayFrameCount(
            true, 10, ref lastObservedFrame, stableFrames);
        stableFrames = GameViewPngCaptureWindow.AdvanceStablePlayFrameCount(
            true, 10, ref lastObservedFrame, stableFrames);
        Assert.That(stableFrames, Is.EqualTo(1));

        stableFrames = GameViewPngCaptureWindow.AdvanceStablePlayFrameCount(
            true, 11, ref lastObservedFrame, stableFrames);
        stableFrames = GameViewPngCaptureWindow.AdvanceStablePlayFrameCount(
            true, 12, ref lastObservedFrame, stableFrames);
        Assert.That(stableFrames, Is.EqualTo(3));
    }

    [Test]
    public void StablePlayFramesResetOnMismatchAndFrameRestart()
    {
        int lastObservedFrame = -1;
        int stableFrames = 0;

        stableFrames = GameViewPngCaptureWindow.AdvanceStablePlayFrameCount(
            true, 20, ref lastObservedFrame, stableFrames);
        stableFrames = GameViewPngCaptureWindow.AdvanceStablePlayFrameCount(
            false, 21, ref lastObservedFrame, stableFrames);
        Assert.That(stableFrames, Is.Zero);

        stableFrames = GameViewPngCaptureWindow.AdvanceStablePlayFrameCount(
            true, 22, ref lastObservedFrame, stableFrames);
        stableFrames = GameViewPngCaptureWindow.AdvanceStablePlayFrameCount(
            true, 24, ref lastObservedFrame, stableFrames);
        Assert.That(stableFrames, Is.EqualTo(2));

        stableFrames = GameViewPngCaptureWindow.AdvanceStablePlayFrameCount(
            true, 0, ref lastObservedFrame, stableFrames);
        Assert.That(stableFrames, Is.Zero);
    }

    [Test]
    public void StateChangingCommandRequiresUnpausedIdlePlayMode()
    {
        Assert.That(
            GameViewPngCaptureWindow.CanDispatchStateChangingCommand(
                true, false, false),
            Is.True);
        Assert.That(
            GameViewPngCaptureWindow.CanDispatchStateChangingCommand(
                false, false, false),
            Is.False);
        Assert.That(
            GameViewPngCaptureWindow.CanDispatchStateChangingCommand(
                true, true, false),
            Is.False);
        Assert.That(
            GameViewPngCaptureWindow.CanDispatchStateChangingCommand(
                true, false, true),
            Is.False);
    }

    [Test]
    public void ManagedPendingCapturePathRequiresOwnedMarkerAndSessionDirectory()
    {
        string validationRoot = Path.Combine(
            Path.GetTempPath(),
            "UnderTheHorizonTests",
            System.Guid.NewGuid().ToString("N"));
        string sessionDirectory = Path.Combine(validationRoot, "session");
        string deeperDirectory = Path.Combine(sessionDirectory, "deeper");
        Directory.CreateDirectory(deeperDirectory);
        try
        {
            string token = System.Guid.NewGuid().ToString("N");
            string pending = Path.Combine(
                sessionDirectory,
                $"capture.png.uth-game-view-capture.{token}.pending.png");
            string otherTool = Path.Combine(
                sessionDirectory,
                $"capture.png.{token}.pending.png");
            string completed = Path.Combine(sessionDirectory, "capture.png");
            string malformed = Path.Combine(
                sessionDirectory,
                "capture.png.uth-game-view-capture.not-a-guid.pending.png");
            string tooDeep = Path.Combine(
                deeperDirectory,
                $"capture.png.uth-game-view-capture.{token}.pending.png");
            string outside = Path.Combine(
                Path.GetDirectoryName(validationRoot) ?? Path.GetTempPath(),
                $"capture.png.uth-game-view-capture.{token}.pending.png");

            Assert.That(
                GameViewPngCaptureWindow.IsManagedPendingCapturePath(validationRoot, pending),
                Is.True);
            Assert.That(
                GameViewPngCaptureWindow.IsManagedPendingCapturePath(validationRoot, otherTool),
                Is.False);
            Assert.That(
                GameViewPngCaptureWindow.IsManagedPendingCapturePath(validationRoot, completed),
                Is.False);
            Assert.That(
                GameViewPngCaptureWindow.IsManagedPendingCapturePath(validationRoot, malformed),
                Is.False);
            Assert.That(
                GameViewPngCaptureWindow.IsManagedPendingCapturePath(validationRoot, tooDeep),
                Is.False);
            Assert.That(
                GameViewPngCaptureWindow.IsManagedPendingCapturePath(validationRoot, outside),
                Is.False);
        }
        finally
        {
            Directory.Delete(validationRoot, true);
        }
    }
}

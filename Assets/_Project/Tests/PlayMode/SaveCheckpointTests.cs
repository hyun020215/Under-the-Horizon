using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class SaveCheckpointTests
{
    private string directory;
    private AppServiceRegistry previousServices;
    private SaveService saves;
    private GameObject owner;
    private GameStateStore state;
    private SaveCheckpoint checkpoint;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(
            Path.GetTempPath(),
            "UnderTheHorizonTests",
            "SaveCheckpoint",
            Guid.NewGuid().ToString("N"));
        saves = new SaveService(directory);
        previousServices = AppContext.Services;
        AppContext.Services = new AppServiceRegistry();
        AppContext.Services.Register(saves);

        owner = new GameObject("SaveCheckpointTests");
        state = owner.AddComponent<GameStateStore>();
        checkpoint = owner.AddComponent<SaveCheckpoint>();
        SetPrivateField(checkpoint, "stateStore", state);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(owner);
        AppContext.Services = previousServices;

        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }

    [Test]
    public void CaptureDoesNothingUntilSlotIsBound()
    {
        state.SetCurrentScene("P-01");

        checkpoint.Capture();

        Assert.That(saves.Exists(new SaveSlot(0)), Is.False);
        Assert.That(Directory.Exists(directory), Is.False);
    }

    [Test]
    public void BoundCaptureUsesRegisteredServiceAndSelectedSlot()
    {
        var slot = new SaveSlot(2);
        state.SetStoryContext("P-02", 0, TimeBlock.Evening);
        state.State.trust["CHR_DANIEL"] = 0;
        checkpoint.Bind(slot);

        checkpoint.Capture();

        Assert.That(saves.Exists(slot), Is.True);
        Assert.That(saves.Exists(new SaveSlot(0)), Is.False);
        GameState loaded = saves.Load(slot);
        Assert.That(loaded.currentStorySceneId, Is.EqualTo("P-02"));
        Assert.That(loaded.trust["CHR_DANIEL"], Is.Zero);
    }

    [Test]
    public void SaveFailureIsReportedWithoutEscapingCapture()
    {
        string fileInsteadOfDirectory = Path.Combine(directory, "not-a-directory");
        Directory.CreateDirectory(directory);
        File.WriteAllText(fileInsteadOfDirectory, "occupied");
        AppContext.Services.Register(new SaveService(fileInsteadOfDirectory));
        checkpoint.Bind(new SaveSlot(1));

        LogAssert.Expect(LogType.Exception, new Regex(".*"));
        Assert.DoesNotThrow(checkpoint.Capture);
    }

    private static void SetPrivateField<T>(object target, string name, T value)
    {
        FieldInfo field = target.GetType().GetField(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field '{name}'.");
        field.SetValue(target, value);
    }
}

using System.IO;
using NUnit.Framework;

public sealed class SaveServiceDeleteTests
{
    private string directory;

    [SetUp]
    public void SetUp() =>
        directory = Path.Combine(Path.GetTempPath(), "uth-save-delete-tests", TestContext.CurrentContext.Test.ID);

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }

    [Test]
    public void DeleteRemovesPrimaryBackupAndTemporaryFiles()
    {
        var saves = new SaveService(directory);
        var slot = new SaveSlot(1);
        saves.Save(slot, new GameState { day = 3 });
        File.WriteAllText(saves.GetPath(slot) + ".bak", "backup");
        File.WriteAllText(saves.GetPath(slot) + ".tmp", "temporary");

        saves.Delete(slot);

        Assert.That(saves.Exists(slot), Is.False);
        Assert.That(File.Exists(saves.GetPath(slot) + ".bak"), Is.False);
        Assert.That(File.Exists(saves.GetPath(slot) + ".tmp"), Is.False);
    }
}

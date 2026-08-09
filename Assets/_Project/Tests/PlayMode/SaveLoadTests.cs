using System;
using System.IO;
using NUnit.Framework;

public sealed class SaveLoadTests
{
    [Test]
    public void SaveServiceRoundTripsLogicalStateInSelectedSlot()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "UnderTheHorizonTests",
            Guid.NewGuid().ToString("N"));

        try
        {
            var saves = new SaveService(directory);
            var state = new GameState
            {
                currentStorySceneId = "D2-02",
                currentLocationId = "LOC_HORIZON",
                day = 2,
                endingId = "ENDING_TEST",
            };
            state.flags.Add("FLAG_TEST");
            state.discoveredEvidence.Add("C-01");
            state.puzzleProgress["PUZ_D2_02"] = "step=2";

            var slot = new SaveSlot(2);
            saves.Save(slot, state);
            GameState loaded = saves.Load(slot);

            Assert.That(loaded.currentStorySceneId, Is.EqualTo("D2-02"));
            Assert.That(loaded.currentLocationId, Is.EqualTo("LOC_HORIZON"));
            Assert.That(loaded.flags, Does.Contain("FLAG_TEST"));
            Assert.That(loaded.discoveredEvidence, Does.Contain("C-01"));
            Assert.That(loaded.puzzleProgress["PUZ_D2_02"], Is.EqualTo("step=2"));
            Assert.That(loaded.endingId, Is.EqualTo("ENDING_TEST"));
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public sealed class SaveData
{
    public int version = SaveVersion.Current;
    public string currentStorySceneId;
    public string currentLocationId;
    public int day;
    public TimeBlock timeBlock;
    public int publicAnxiety;
    public int evidenceIntegrity;
    public List<StringIntPair> trust = new();
    public List<string> flags = new();
    public List<string> discoveredEvidence = new();
    public List<string> completedInteractions = new();
    public List<string> completedPuzzles = new();
    public List<string> completedStoryScenes = new();
    public List<string> completedObjectives = new();
    public List<string> unlockedLocations = new();
    public List<StringPair> puzzleProgress = new();
    public string endingId;

    public static SaveData FromState(GameState state) => new()
    {
        currentStorySceneId = state.currentStorySceneId,
        currentLocationId = state.currentLocationId,
        day = state.day,
        timeBlock = state.timeBlock,
        publicAnxiety = state.publicAnxiety,
        evidenceIntegrity = state.evidenceIntegrity,
        trust = state.trust.Select(pair => new StringIntPair(pair.Key, pair.Value)).ToList(),
        flags = state.flags.ToList(), discoveredEvidence = state.discoveredEvidence.ToList(),
        completedInteractions = state.completedInteractions.ToList(), completedPuzzles = state.completedPuzzles.ToList(),
        completedStoryScenes = state.completedStoryScenes.ToList(), completedObjectives = state.completedObjectives.ToList(),
        unlockedLocations = state.unlockedLocations.ToList(),
        puzzleProgress = state.puzzleProgress.Select(pair => new StringPair(pair.Key, pair.Value)).ToList(),
        endingId = state.endingId
    };

    public GameState ToState()
    {
        var result = new GameState { currentStorySceneId = currentStorySceneId ?? string.Empty, currentLocationId = currentLocationId ?? string.Empty, day = day, timeBlock = timeBlock, publicAnxiety = publicAnxiety, evidenceIntegrity = evidenceIntegrity, endingId = endingId ?? string.Empty };
        foreach (StringIntPair pair in trust ?? new()) result.trust[pair.key] = pair.value;
        Add(result.flags, flags); Add(result.discoveredEvidence, discoveredEvidence); Add(result.completedInteractions, completedInteractions);
        Add(result.completedPuzzles, completedPuzzles); Add(result.completedStoryScenes, completedStoryScenes); Add(result.completedObjectives, completedObjectives); Add(result.unlockedLocations, unlockedLocations);
        foreach (StringPair pair in puzzleProgress ?? new()) result.puzzleProgress[pair.key] = pair.value;
        return result;
    }

    private static void Add(HashSet<string> target, IEnumerable<string> values) { if (values == null) return; foreach (string value in values) if (!string.IsNullOrWhiteSpace(value)) target.Add(value); }
}

[Serializable] public struct StringIntPair { public string key; public int value; public StringIntPair(string key, int value) { this.key = key; this.value = value; } }
[Serializable] public struct StringPair { public string key; public string value; public StringPair(string key, string value) { this.key = key; this.value = value; } }

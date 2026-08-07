using System;
using System.Collections.Generic;

[Serializable]
public sealed class GameState
{
    public string currentStorySceneId = string.Empty;
    public string currentLocationId = string.Empty;
    public int day;
    public TimeBlock timeBlock;
    public int publicAnxiety;
    public int evidenceIntegrity = 100;
    public Dictionary<string, int> trust = new(StringComparer.Ordinal);
    public HashSet<string> flags = new(StringComparer.Ordinal);
    public HashSet<string> discoveredEvidence = new(StringComparer.Ordinal);
    public HashSet<string> completedInteractions = new(StringComparer.Ordinal);
    public HashSet<string> completedPuzzles = new(StringComparer.Ordinal);
    public HashSet<string> completedStoryScenes = new(StringComparer.Ordinal);
    public HashSet<string> completedObjectives = new(StringComparer.Ordinal);
    public HashSet<string> unlockedLocations = new(StringComparer.Ordinal);
    public Dictionary<string, string> puzzleProgress = new(StringComparer.Ordinal);
    public string endingId = string.Empty;

    public GameState Clone()
    {
        return new GameState
        {
            currentStorySceneId = currentStorySceneId,
            currentLocationId = currentLocationId,
            day = day,
            timeBlock = timeBlock,
            publicAnxiety = publicAnxiety,
            evidenceIntegrity = evidenceIntegrity,
            trust = new Dictionary<string, int>(trust, StringComparer.Ordinal),
            flags = new HashSet<string>(flags, StringComparer.Ordinal),
            discoveredEvidence = new HashSet<string>(discoveredEvidence, StringComparer.Ordinal),
            completedInteractions = new HashSet<string>(
                completedInteractions,
                StringComparer.Ordinal
            ),
            completedPuzzles = new HashSet<string>(completedPuzzles, StringComparer.Ordinal),
            completedStoryScenes = new HashSet<string>(
                completedStoryScenes,
                StringComparer.Ordinal
            ),
            completedObjectives = new HashSet<string>(completedObjectives, StringComparer.Ordinal),
            unlockedLocations = new HashSet<string>(unlockedLocations, StringComparer.Ordinal),
            puzzleProgress = new Dictionary<string, string>(puzzleProgress, StringComparer.Ordinal),
            endingId = endingId,
        };
    }
}

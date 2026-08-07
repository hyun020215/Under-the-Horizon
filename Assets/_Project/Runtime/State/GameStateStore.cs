using System;
using UnityEngine;

public sealed class GameStateStore : MonoBehaviour
{
    [SerializeField]
    private GameState state = new();
    public GameState State => state;
    public event Action<GameState> Changed;

    public void Replace(GameState replacement)
    {
        state = replacement?.Clone() ?? new GameState();
        Notify();
    }

    public void SetCurrentScene(string sceneId)
    {
        state.currentStorySceneId = Normalize(sceneId);
        Notify();
    }

    public void SetCurrentLocation(string locationId)
    {
        state.currentLocationId = Normalize(locationId);
        if (!string.IsNullOrEmpty(state.currentLocationId))
            state.unlockedLocations.Add(state.currentLocationId);
        Notify();
    }

    public bool HasFlag(string id) => state.flags.Contains(Normalize(id));

    public bool SetFlag(string id, bool value = true) => SetMembership(state.flags, id, value);

    public bool HasEvidence(string id) => state.discoveredEvidence.Contains(Normalize(id));

    public bool AddEvidence(string id) => Add(state.discoveredEvidence, id);

    public bool IsInteractionCompleted(string id) =>
        state.completedInteractions.Contains(Normalize(id));

    public bool CompleteInteraction(string id) => Add(state.completedInteractions, id);

    public bool IsPuzzleCompleted(string id) => state.completedPuzzles.Contains(Normalize(id));

    public bool CompletePuzzle(string id) => Add(state.completedPuzzles, id);

    public bool IsSceneCompleted(string id) => state.completedStoryScenes.Contains(Normalize(id));

    public bool CompleteScene(string id) => Add(state.completedStoryScenes, id);

    public bool CompleteObjective(string id) => Add(state.completedObjectives, id);

    public bool UnlockLocation(string id) => Add(state.unlockedLocations, id);

    public int GetTrust(string characterId) =>
        state.trust.TryGetValue(Normalize(characterId), out int value) ? value : 0;

    public void ModifyTrust(string characterId, int delta)
    {
        string id = Normalize(characterId);
        if (string.IsNullOrEmpty(id))
            return;
        state.trust[id] = Mathf.Clamp(GetTrust(id) + delta, -100, 100);
        Notify();
    }

    public void ChangeAnxiety(int delta)
    {
        state.publicAnxiety = Mathf.Clamp(state.publicAnxiety + delta, 0, 100);
        Notify();
    }

    public void ChangeIntegrity(int delta)
    {
        state.evidenceIntegrity = Mathf.Clamp(state.evidenceIntegrity + delta, 0, 100);
        Notify();
    }

    public void SetPuzzleProgress(string id, string payload)
    {
        state.puzzleProgress[Normalize(id)] = payload ?? string.Empty;
        Notify();
    }

    public bool TryGetPuzzleProgress(string id, out string payload) =>
        state.puzzleProgress.TryGetValue(Normalize(id), out payload);

    private bool Add(System.Collections.Generic.HashSet<string> set, string value)
    {
        string id = Normalize(value);
        bool changed = !string.IsNullOrEmpty(id) && set.Add(id);
        if (changed)
            Notify();
        return changed;
    }

    private bool SetMembership(
        System.Collections.Generic.HashSet<string> set,
        string value,
        bool enabled
    )
    {
        string id = Normalize(value);
        if (string.IsNullOrEmpty(id))
            return false;
        bool changed = enabled ? set.Add(id) : set.Remove(id);
        if (changed)
            Notify();
        return changed;
    }

    private void Notify() => Changed?.Invoke(state);

    private static string Normalize(string value) => value?.Trim() ?? string.Empty;
}

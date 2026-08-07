using System;
using UnityEngine;

public static class SaveSerializer
{
    public static string Serialize(GameState state) => JsonUtility.ToJson(SaveData.FromState(state ?? new GameState()), true);
    public static GameState Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new GameState();
        SaveData data = JsonUtility.FromJson<SaveData>(json) ?? throw new InvalidOperationException("Invalid save data.");
        return SaveMigrationRegistry.Migrate(data).ToState();
    }
}

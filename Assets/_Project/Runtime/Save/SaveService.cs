using System;
using System.IO;
using UnityEngine;

public sealed class SaveService
{
    private readonly string directory;

    public SaveService(string directory = null)
    {
        this.directory = directory ?? Path.Combine(Application.persistentDataPath, "Saves");
    }

    public string GetPath(SaveSlot slot) => Path.Combine(directory, slot + ".json");

    public bool Exists(SaveSlot slot) => File.Exists(GetPath(slot));

    public void Delete(SaveSlot slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path))
            File.Delete(path);
        string backup = path + ".bak";
        if (File.Exists(backup))
            File.Delete(backup);
        string temporary = path + ".tmp";
        if (File.Exists(temporary))
            File.Delete(temporary);
    }

    public void Save(SaveSlot slot, GameState state)
    {
        Directory.CreateDirectory(directory);
        string path = GetPath(slot);
        string temporary = path + ".tmp";
        string backup = path + ".bak";
        File.WriteAllText(temporary, SaveSerializer.Serialize(state));
        if (File.Exists(path))
            File.Replace(temporary, path, backup);
        else
            File.Move(temporary, path);
    }

    public GameState Load(SaveSlot slot)
    {
        string path = GetPath(slot);
        if (!File.Exists(path))
            return new GameState();
        try
        {
            return SaveSerializer.Deserialize(File.ReadAllText(path));
        }
        catch (Exception) when (File.Exists(path + ".bak"))
        {
            return SaveSerializer.Deserialize(File.ReadAllText(path + ".bak"));
        }
    }
}

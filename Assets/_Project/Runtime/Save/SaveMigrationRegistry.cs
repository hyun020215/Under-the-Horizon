using System;
using System.Collections.Generic;

public static class SaveMigrationRegistry
{
    private static readonly List<SaveMigration> Migrations = new();

    public static void Register(SaveMigration migration)
    {
        if (migration != null && !Migrations.Contains(migration))
            Migrations.Add(migration);
    }

    public static SaveData Migrate(SaveData data)
    {
        if (data.version > SaveVersion.Current)
            throw new InvalidOperationException(
                $"Save version {data.version} is newer than supported version {SaveVersion.Current}."
            );
        while (data.version < SaveVersion.Current)
        {
            SaveMigration migration =
                Migrations.Find(item => item.FromVersion == data.version)
                ?? throw new InvalidOperationException(
                    $"No migration from save version {data.version}."
                );
            data = migration.Apply(data);
            data.version = migration.ToVersion;
        }
        return data;
    }
}

using System;

public sealed class SaveMigrationV1ToV2 : SaveMigration
{
    public override int FromVersion => 1;
    public override int ToVersion => 2;

    public override SaveData Apply(SaveData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        data.pendingStorySceneId ??= string.Empty;
        return data;
    }
}

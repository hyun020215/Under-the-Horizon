public abstract class SaveMigration
{
    public abstract int FromVersion { get; }
    public abstract int ToVersion { get; }
    public abstract SaveData Apply(SaveData data);
}

public readonly struct SaveSlot
{
    public SaveSlot(int index) { Index = index < 0 ? 0 : index; }
    public int Index { get; }
    public override string ToString() => $"slot_{Index:D2}";
}

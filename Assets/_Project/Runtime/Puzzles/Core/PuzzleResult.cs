public readonly struct PuzzleResult
{
    public PuzzleResult(bool completed, string payload = null)
    {
        Completed = completed;
        Payload = payload ?? string.Empty;
    }

    public bool Completed { get; }
    public string Payload { get; }
    public static PuzzleResult Cancelled => new(false);
}

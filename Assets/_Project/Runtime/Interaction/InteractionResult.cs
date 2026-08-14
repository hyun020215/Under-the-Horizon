public readonly struct InteractionResult
{
    public InteractionResult(
        bool success,
        string message = null,
        bool advanceStorySceneRequested = false)
    {
        Success = success;
        Message = message ?? string.Empty;
        AdvanceStorySceneRequested = advanceStorySceneRequested;
    }

    public bool Success { get; }
    public string Message { get; }
    public bool AdvanceStorySceneRequested { get; }
    public static InteractionResult Completed => new(true);
    public static InteractionResult CompletedWithStorySceneAdvance =>
        new(true, advanceStorySceneRequested: true);
}

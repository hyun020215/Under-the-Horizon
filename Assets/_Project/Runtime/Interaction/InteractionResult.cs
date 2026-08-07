public readonly struct InteractionResult
{
    public InteractionResult(bool success, string message = null)
    {
        Success = success;
        Message = message ?? string.Empty;
    }

    public bool Success { get; }
    public string Message { get; }
    public static InteractionResult Completed => new(true);
}

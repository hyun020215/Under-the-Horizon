public readonly struct ScreenContext
{
    public ScreenContext(object payload)
    {
        Payload = payload;
    }

    public object Payload { get; }

    public T Get<T>() => Payload is T value ? value : default;
}

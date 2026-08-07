public readonly struct PuzzleContext
{
    public PuzzleContext(PuzzleDefinition definition, GameStateStore state)
    {
        Definition = definition;
        State = state;
    }

    public PuzzleDefinition Definition { get; }
    public GameStateStore State { get; }
}

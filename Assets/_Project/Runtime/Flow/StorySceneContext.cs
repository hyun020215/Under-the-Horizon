public readonly struct StorySceneContext
{
    public StorySceneContext(StorySceneDefinition definition, GameStateStore state)
    {
        Definition = definition;
        State = state;
    }

    public StorySceneDefinition Definition { get; }
    public GameStateStore State { get; }
}

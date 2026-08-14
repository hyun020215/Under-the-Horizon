public readonly struct PendingStorySceneTravel
{
    public PendingStorySceneTravel(
        StorySceneDefinition sourceScene,
        StorySceneDefinition targetScene)
    {
        SourceScene = sourceScene;
        TargetScene = targetScene;
    }

    public StorySceneDefinition SourceScene { get; }
    public StorySceneDefinition TargetScene { get; }
    public LocationDefinition Destination => TargetScene?.Location;
    public string TargetSceneId => TargetScene?.Id ?? string.Empty;
    public string DestinationId => Destination?.Id ?? string.Empty;
}

public readonly struct StorySceneTravelResult
{
    public StorySceneTravelResult(
        bool success,
        PendingStorySceneTravel travel,
        string message = null)
    {
        Success = success;
        Travel = travel;
        Message = message ?? string.Empty;
    }

    public bool Success { get; }
    public PendingStorySceneTravel Travel { get; }
    public string Message { get; }
}

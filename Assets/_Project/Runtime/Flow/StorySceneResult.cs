public readonly struct StorySceneResult
{
    public StorySceneResult(
        bool completed,
        string nextSceneId = null,
        StorySceneAdvanceMode advanceMode = StorySceneAdvanceMode.Immediate)
    {
        Completed = completed;
        NextSceneId = nextSceneId ?? string.Empty;
        AdvanceMode = advanceMode;
    }

    public bool Completed { get; }
    public string NextSceneId { get; }
    public StorySceneAdvanceMode AdvanceMode { get; }
}

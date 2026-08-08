using System.Threading.Tasks;
using UnityEngine;

public sealed class GameFlowController : MonoBehaviour
{
    [SerializeField]
    private ContentDatabase content;

    [SerializeField]
    private StorySceneDirector scenes;

    [SerializeField]
    private GameStateStore state;

    public async Task StartAsync(string sceneId)
    {
        if (content == null || !content.TryGetStoryScene(sceneId, out var scene))
            throw new System.InvalidOperationException($"Story Scene '{sceneId}' is missing.");
        await scenes.EnterAsync(scene);
    }

    public async Task AdvanceAsync()
    {
        StorySceneResult result = await scenes.CompleteAsync();
        if (result.Completed && !string.IsNullOrWhiteSpace(result.NextSceneId))
            await StartAsync(result.NextSceneId);
    }

    public Task ResumeAsync() => StartAsync(state.State.currentStorySceneId);

    public bool CanAdvance => scenes != null
        && scenes.Current != null
        && !string.IsNullOrWhiteSpace(scenes.Current.ResolveNext(state));
}

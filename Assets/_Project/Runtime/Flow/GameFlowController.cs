using System;
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

    private bool isChangingFlow;
    public event Action ProgressCheckpointReached;

    public async Task StartAsync(string sceneId)
    {
        if (isChangingFlow)
            throw new InvalidOperationException("A Story Scene flow change is already running.");

        GameState snapshot = state?.State.Clone();
        StorySceneDefinition source = scenes?.Current;
        isChangingFlow = true;
        try
        {
            StorySceneDefinition scene = ResolveScene(sceneId);
            state?.ClearPendingStoryScene();
            await scenes.EnterAsync(scene);
        }
        catch
        {
            if (snapshot != null)
                state.Replace(snapshot);
            await TryRestoreAsync(source);
            throw;
        }
        finally
        {
            isChangingFlow = false;
        }
    }

    public async Task AdvanceAsync()
    {
        if (isChangingFlow)
            throw new InvalidOperationException("A Story Scene flow change is already running.");
        if (!TryValidateAdvance(out _, out _, out string reason))
            throw new InvalidOperationException(reason);

        GameState snapshot = state.State.Clone();
        StorySceneDefinition source = scenes.Current;
        isChangingFlow = true;
        try
        {
            StorySceneResult result = await scenes.CompleteAsync();
            if (!result.Completed || string.IsNullOrWhiteSpace(result.NextSceneId))
                throw new InvalidOperationException("The current Story Scene did not resolve a target route.");

            StorySceneDefinition target = ResolveScene(result.NextSceneId);
            if (!ConditionResolver.All(target.EntryConditions, state))
                throw new InvalidOperationException(
                    $"Entry conditions failed for '{target.Id}' after completing '{source.Id}'.");

            if (result.AdvanceMode == StorySceneAdvanceMode.MapTravel)
            {
                if (target.Location == null)
                    throw new InvalidOperationException(
                        $"Map-travel target Story Scene '{target.Id}' has no Location.");

                state.SetPendingStoryScene(target.Id);
                ProgressCheckpointReached?.Invoke();
                return;
            }

            state.ClearPendingStoryScene();
            await scenes.EnterAsync(target);
        }
        catch
        {
            state.Replace(snapshot);
            await TryRestoreAsync(source);
            throw;
        }
        finally
        {
            isChangingFlow = false;
        }
    }

    public async Task ResumeAsync()
    {
        if (isChangingFlow)
            throw new InvalidOperationException("A Story Scene flow change is already running.");
        if (state == null)
            throw new InvalidOperationException("GameFlowController requires a GameStateStore.");

        string currentSceneId = state.State.currentStorySceneId;
        if (string.IsNullOrWhiteSpace(currentSceneId))
            throw new InvalidOperationException("The loaded save has no current Story Scene.");

        StorySceneDefinition current = ResolveScene(currentSceneId);
        string pendingSceneId = state.State.pendingStorySceneId;
        if (!string.IsNullOrWhiteSpace(pendingSceneId))
        {
            if (!TryGetPendingTravel(out PendingStorySceneTravel pendingTravel))
                throw new InvalidOperationException(
                    $"Pending travel from Story Scene '{current.Id}' to "
                        + $"'{pendingSceneId}' does not match the current map-travel route.");

            isChangingFlow = true;
            try
            {
                await scenes.RestorePresentationAsync(pendingTravel.SourceScene);
            }
            finally
            {
                isChangingFlow = false;
            }
            return;
        }

        if (state.IsSceneCompleted(current.Id))
        {
            StorySceneRoute route = current.ResolveRoute(state);
            if (route == null)
            {
                isChangingFlow = true;
                try
                {
                    await scenes.RestorePresentationAsync(current);
                }
                finally
                {
                    isChangingFlow = false;
                }
                return;
            }

            StorySceneDefinition target = ResolveScene(route.TargetSceneId);
            if (route.AdvanceMode == StorySceneAdvanceMode.MapTravel)
            {
                if (target.Location == null)
                    throw new InvalidOperationException(
                        $"Map-travel target Story Scene '{target.Id}' has no Location.");
                state.SetPendingStoryScene(target.Id);
                ProgressCheckpointReached?.Invoke();
                isChangingFlow = true;
                try
                {
                    await scenes.RestorePresentationAsync(current);
                }
                finally
                {
                    isChangingFlow = false;
                }
                return;
            }

            await StartAsync(target.Id);
            return;
        }

        await StartAsync(current.Id);
    }

    public bool TryGetPendingTravel(out PendingStorySceneTravel travel)
    {
        travel = default;
        if (state == null || content == null)
            return false;

        string targetSceneId = state.State.pendingStorySceneId;
        if (string.IsNullOrWhiteSpace(targetSceneId)
            || !content.TryGetStoryScene(targetSceneId, out StorySceneDefinition target)
            || target == null
            || target.Location == null)
        {
            return false;
        }

        if (!content.TryGetStoryScene(
                state.State.currentStorySceneId,
                out StorySceneDefinition source)
            || source == null
            || !state.IsSceneCompleted(source.Id))
        {
            return false;
        }

        StorySceneRoute route = source.ResolveRoute(state);
        if (route == null
            || route.AdvanceMode != StorySceneAdvanceMode.MapTravel
            || !string.Equals(route.TargetSceneId, targetSceneId, StringComparison.Ordinal))
        {
            return false;
        }

        travel = new PendingStorySceneTravel(source, target);
        return true;
    }

    public bool CanTravelTo(string locationId) => CanTravelTo(locationId, out _);

    public bool CanTravelTo(string locationId, out string reason)
    {
        if (isChangingFlow)
        {
            reason = "A Story Scene flow change is already running.";
            return false;
        }
        if (!TryGetPendingTravel(out PendingStorySceneTravel travel))
        {
            reason = "There is no valid pending Story Scene travel.";
            return false;
        }
        if (travel.SourceScene == null
            || !string.Equals(
                state.State.currentStorySceneId,
                travel.SourceScene.Id,
                StringComparison.Ordinal)
            || !state.IsSceneCompleted(travel.SourceScene.Id))
        {
            reason = "The pending travel source is not the completed current Story Scene.";
            return false;
        }
        if (!string.Equals(locationId, travel.DestinationId, StringComparison.Ordinal))
        {
            reason = $"Location '{locationId}' is not the pending destination.";
            return false;
        }
        if (!ConditionResolver.All(travel.TargetScene.EntryConditions, state))
        {
            reason = $"Entry conditions failed for '{travel.TargetSceneId}'.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public async Task<StorySceneTravelResult> TravelAsync(string locationId)
    {
        if (!CanTravelTo(locationId, out string reason))
            return new StorySceneTravelResult(false, default, reason);

        TryGetPendingTravel(out PendingStorySceneTravel travel);
        GameState snapshot = state.State.Clone();
        isChangingFlow = true;
        try
        {
            state.ClearPendingStoryScene();
            await scenes.EnterAsync(travel.TargetScene);
            return new StorySceneTravelResult(true, travel);
        }
        catch (Exception exception)
        {
            state.Replace(snapshot);
            await TryRestoreAsync(travel.SourceScene);
            return new StorySceneTravelResult(false, travel, exception.Message);
        }
        finally
        {
            isChangingFlow = false;
        }
    }

    public bool TryValidateAdvance(out string reason) =>
        TryValidateAdvance(out _, out _, out reason);

    public bool CanAdvance => TryValidateAdvance(out _);

    private bool TryValidateAdvance(
        out StorySceneRoute route,
        out StorySceneDefinition target,
        out string reason)
    {
        route = null;
        target = null;
        if (isChangingFlow)
        {
            reason = "A Story Scene flow change is already running.";
            return false;
        }
        if (content == null || scenes == null || state == null)
        {
            reason = "GameFlowController is missing required references.";
            return false;
        }
        if (scenes.Current == null)
        {
            reason = "There is no current Story Scene to advance.";
            return false;
        }
        if (!string.IsNullOrWhiteSpace(state.State.pendingStorySceneId))
        {
            reason = "The current Story Scene already has pending travel.";
            return false;
        }

        GameObject previewOwner = new("Story Scene Completion Preview")
        {
            hideFlags = HideFlags.HideAndDontSave,
        };
        try
        {
            GameStateStore preview = previewOwner.AddComponent<GameStateStore>();
            preview.Replace(state.State);
            Apply(scenes.Current.OnCompleteEffects, preview);
            preview.CompleteScene(scenes.Current.Id);
            route = scenes.Current.ResolveRoute(preview);
            if (route == null || string.IsNullOrWhiteSpace(route.TargetSceneId))
            {
                reason = $"Story Scene '{scenes.Current.Id}' has no available target route.";
                return false;
            }
            if (!content.TryGetStoryScene(route.TargetSceneId, out target) || target == null)
            {
                reason = $"Story Scene '{route.TargetSceneId}' is missing.";
                return false;
            }
            if (!ConditionResolver.All(target.EntryConditions, preview))
            {
                reason = $"Entry conditions failed for '{target.Id}'.";
                return false;
            }
            if (route.AdvanceMode == StorySceneAdvanceMode.MapTravel && target.Location == null)
            {
                reason = $"Map-travel target Story Scene '{target.Id}' has no Location.";
                return false;
            }

            reason = string.Empty;
            return true;
        }
        finally
        {
            DestroyImmediate(previewOwner);
        }
    }

    private StorySceneDefinition ResolveScene(string sceneId)
    {
        if (content == null || !content.TryGetStoryScene(sceneId, out StorySceneDefinition scene))
            throw new InvalidOperationException($"Story Scene '{sceneId}' is missing.");
        if (scenes == null)
            throw new InvalidOperationException("GameFlowController requires a StorySceneDirector.");
        return scene;
    }

    private async Task TryRestoreAsync(StorySceneDefinition scene)
    {
        if (scene == null || scenes == null)
            return;
        try
        {
            await scenes.RestorePresentationAsync(scene);
        }
        catch
        {
            // Preserve the original flow error. Logical state has already been restored.
        }
    }

    private static void Apply(GameEffect[] effects, GameStateStore targetState)
    {
        if (effects == null)
            return;
        foreach (GameEffect effect in effects)
            effect?.Apply(targetState);
    }
}

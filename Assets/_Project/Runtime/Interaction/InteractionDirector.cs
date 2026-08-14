using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class InteractionDirector : MonoBehaviour
{
    [SerializeField]
    private GameStateStore state;

    [SerializeField]
    private NarrativeDirector narrative;

    [SerializeField]
    private PuzzleDirector puzzles;

    [SerializeField]
    private GameFlowController flow;

    [SerializeField]
    private InteractionPointView hotspotPrefab;

    [SerializeField]
    private RectTransform hotspotRoot;

    private readonly List<InteractionPointView> hotspotViews = new();
    private bool isExecuting;
    public InteractionSet Current { get; private set; }

    public void Apply(InteractionSet set)
    {
        ClearHotspots();
        Current = set;

        if (set?.Interactions == null || hotspotPrefab == null || hotspotRoot == null)
            return;

        foreach (InteractionDefinition definition in set.Interactions)
        {
            if (definition == null || !definition.HasWorldHotspot)
                continue;

            InteractionPointView view = Instantiate(hotspotPrefab, hotspotRoot);
            view.Apply(definition);
            view.Clicked += OnHotspotClicked;
            hotspotViews.Add(view);
        }

        RefreshHotspots();
    }

    public async Task<InteractionResult> ExecuteFirstAvailableAsync(
        InteractionType preferredType,
        string targetId = null)
    {
        InteractionDefinition fallback = null;
        if (Current?.Interactions != null)
        {
            foreach (InteractionDefinition definition in Current.Interactions)
            {
                if (definition == null
                    || !definition.IsAvailable(state)
                    || !definition.MatchesTarget(targetId))
                    continue;
                if (definition.Type == preferredType)
                    return await ExecuteAsync(definition);
                fallback ??= definition;
            }
        }
        return fallback != null
            ? await ExecuteAsync(fallback)
            : new InteractionResult(false, "Unavailable");
    }

    public async Task<InteractionResult> ExecuteAsync(InteractionDefinition definition)
    {
        if (isExecuting)
            return new InteractionResult(false, "Busy");

        isExecuting = true;
        try
        {
            if (definition == null
                || !definition.IsAvailable(state)
                || definition.Action == null)
            {
                return new InteractionResult(false, "Unavailable");
            }

            InteractionResult result = await definition.Action.ExecuteAsync(
                new InteractionContext(state, narrative, puzzles)
            );
            if (result.Success && !definition.Repeatable)
                state?.CompleteInteraction(definition.Id);

            RefreshHotspots();

            if (result.Success && result.AdvanceStorySceneRequested)
            {
                if (flow == null)
                {
                    throw new InvalidOperationException(
                        "InteractionDirector requires a GameFlowController "
                        + "to advance the current Story Scene.");
                }

                await flow.AdvanceAsync();
            }

            return result;
        }
        finally
        {
            isExecuting = false;
            RefreshHotspots();
        }
    }

    private void ClearHotspots()
    {
        foreach (InteractionPointView view in hotspotViews)
        {
            if (view == null)
                continue;
            view.Clicked -= OnHotspotClicked;
            Destroy(view.gameObject);
        }
        hotspotViews.Clear();
    }

    private async void OnHotspotClicked(InteractionPointView view)
    {
        try
        {
            await ExecuteAsync(view.Definition);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, view);
        }
    }

    private void RefreshHotspots()
    {
        foreach (InteractionPointView view in hotspotViews)
        {
            if (view?.Definition != null)
                view.gameObject.SetActive(view.Definition.IsAvailable(state));
        }
    }
}

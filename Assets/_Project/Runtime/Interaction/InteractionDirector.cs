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
    public event Action AvailabilityChanged;

    private void OnEnable()
    {
        if (state != null)
            state.Changed += OnStateChanged;
        RefreshAvailability();
    }

    private void OnDisable()
    {
        if (state != null)
            state.Changed -= OnStateChanged;
    }

    public void Apply(InteractionSet set)
    {
        ClearHotspots();
        Current = set;

        if (set?.Interactions != null && hotspotPrefab != null && hotspotRoot != null)
        {
            foreach (InteractionDefinition definition in set.Interactions)
            {
                if (definition == null || !definition.HasWorldHotspot)
                    continue;

                InteractionPointView view = Instantiate(hotspotPrefab, hotspotRoot);
                view.Apply(definition);
                view.Clicked += OnHotspotClicked;
                hotspotViews.Add(view);
            }
        }

        RefreshAvailability();
    }

    public bool TryGetFirstAvailable(
        InteractionType type,
        string targetId,
        out InteractionDefinition definition)
    {
        definition = null;
        if (Current?.Interactions == null)
            return false;

        foreach (InteractionDefinition candidate in Current.Interactions)
        {
            if (candidate == null
                || candidate.Type != type
                || !string.Equals(
                    candidate.TargetId,
                    targetId,
                    StringComparison.Ordinal)
                || !candidate.IsAvailable(state))
            {
                continue;
            }

            definition = candidate;
            return true;
        }

        return false;
    }

    public bool TryGetFirstAvailableAnchored(
        InteractionType type,
        string exactTargetId,
        out InteractionDefinition definition)
    {
        definition = null;
        if (Current?.Interactions == null || string.IsNullOrWhiteSpace(exactTargetId))
            return false;

        foreach (InteractionDefinition candidate in Current.Interactions)
        {
            if (candidate == null
                || candidate.Type != type
                || candidate.HasWorldHotspot
                || !string.Equals(
                    candidate.TargetId,
                    exactTargetId,
                    StringComparison.Ordinal)
                || !candidate.IsAvailable(state))
            {
                continue;
            }

            definition = candidate;
            return true;
        }

        return false;
    }

    public async Task<InteractionResult> ExecuteFirstAvailableAsync(
        InteractionType type,
        string targetId = null)
    {
        return TryGetFirstAvailable(type, targetId, out InteractionDefinition definition)
            ? await ExecuteAsync(definition)
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

            RefreshAvailability();

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
            RefreshAvailability();
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
            if (view != null && view.Definition != null)
                view.gameObject.SetActive(view.Definition.IsAvailable(state));
        }
    }

    private void RefreshAvailability()
    {
        RefreshHotspots();
        AvailabilityChanged?.Invoke();
    }

    private void OnStateChanged(GameState _) => RefreshAvailability();
}

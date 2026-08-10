using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class ScreenRouter : MonoBehaviour
{
    [SerializeField]
    private ScreenBase[] screens;
    [SerializeField]
    private TransitionDirector transitionDirector;
    [SerializeField]
    private TransitionProfile defaultTransition;
    private readonly Dictionary<ScreenId, ScreenBase> index = new();
    public ScreenId? Current { get; private set; }
    public event Action<ScreenId> Opened;

    private void Awake()
    {
        index.Clear();
        if (screens != null)
            foreach (var screen in screens)
                if (screen != null)
                    index[screen.Id] = screen;
    }

    public async Task OpenAsync(ScreenId id, ScreenContext context = default)
    {
        await OpenAsync(id, context, transitionDirector, defaultTransition);
    }

    public async Task OpenAsync(
        ScreenId id,
        ScreenContext context,
        TransitionDirector transitions,
        TransitionProfile transition)
    {
        if (!index.TryGetValue(id, out var next))
            throw new InvalidOperationException($"Screen {id} is not registered.");

        if (Current == id)
        {
            await next.OpenAsync(context);
            return;
        }

        bool playTransition = transitions != null && transition != null;
        if (playTransition)
            await transitions.BeginAsync(transition);
        try
        {
            if (Current.HasValue && index.TryGetValue(Current.Value, out var current))
                await current.CloseAsync();
            await next.OpenAsync(context);
            Current = id;
            Opened?.Invoke(id);
        }
        finally
        {
            if (playTransition)
                await transitions.EndAsync(transition);
        }
    }

    public Task OpenAsync(ScreenMode mode, ScreenContext context = default) =>
        OpenAsync((ScreenId)mode, context);
}

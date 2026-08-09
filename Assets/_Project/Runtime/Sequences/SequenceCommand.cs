using System;
using System.Threading.Tasks;

[Serializable]
public abstract class SequenceCommand
{
    public abstract Task ExecuteAsync(SequenceContext context);
}

public readonly struct SequenceContext
{
    public SequenceContext(
        GameStateStore state,
        NarrativeDirector narrative,
        AudioDirector audio,
        TransitionDirector transitions,
        ScreenRouter screens,
        UIInputBlocker inputBlocker,
        CinematicOverlayPresenter cinematicOverlay
    )
    {
        State = state;
        Narrative = narrative;
        Audio = audio;
        Transitions = transitions;
        Screens = screens;
        InputBlocker = inputBlocker;
        CinematicOverlay = cinematicOverlay;
    }

    public GameStateStore State { get; }
    public NarrativeDirector Narrative { get; }
    public AudioDirector Audio { get; }
    public TransitionDirector Transitions { get; }
    public ScreenRouter Screens { get; }
    public UIInputBlocker InputBlocker { get; }
    public CinematicOverlayPresenter CinematicOverlay { get; }
}

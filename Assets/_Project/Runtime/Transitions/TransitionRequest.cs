public readonly struct TransitionRequest
{
    public TransitionRequest(TransitionProfile profile, bool entering, bool reducedMotion = false)
    {
        Profile = profile;
        Entering = entering;
        ReducedMotion = reducedMotion;
    }

    public TransitionProfile Profile { get; }
    public bool Entering { get; }
    public bool ReducedMotion { get; }
}

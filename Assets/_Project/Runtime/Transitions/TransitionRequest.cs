public readonly struct TransitionRequest
{
    public TransitionRequest(TransitionProfile profile, bool entering)
    {
        Profile = profile;
        Entering = entering;
    }

    public TransitionProfile Profile { get; }
    public bool Entering { get; }
}

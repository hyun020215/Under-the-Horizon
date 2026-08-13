using UnityEngine;

[CreateAssetMenu(fileName = "TRANS_", menuName = "Under The Horizon/Transitions/Profile")]
public sealed class TransitionProfile : ScriptableObject
{
    public TransitionType type;

    [Min(0)]
    public float uiExitDuration = .15f;

    [Min(0)]
    public float coverDuration = .25f;

    [Min(0)]
    public float holdDuration;

    [Min(0)]
    public float revealDuration = .25f;

    [Min(0)]
    public float uiEnterDuration = .15f;
    [Header("Cover presentation")]
    public Color coverColor = new(.015f, .025f, .07f, 1f);
    public Color particleColor = new(.92f, .76f, .42f, .42f);
    [Range(0, 24)] public int particleCount = 12;
    public AudioClip stinger;
    public bool blockInput = true;
}

using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Effects/Set Flag")]
public sealed class SetFlagEffect : GameEffect
{
    [SerializeField]
    private string flagId;

    [SerializeField]
    private bool value = true;

    public override void Apply(GameStateStore state) => state?.SetFlag(flagId, value);
}

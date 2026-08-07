using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Has Flag")]
public sealed class HasFlagCondition : Condition
{
    [SerializeField] private string flagId;
    [SerializeField] private bool expected = true;
    public override bool Evaluate(GameStateStore state) => state != null && state.HasFlag(flagId) == expected;
}

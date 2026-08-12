using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Ending")]
public sealed class EndingCondition : Condition
{
    [SerializeField]
    private string endingId;

    public override bool Evaluate(GameStateStore state) =>
        state != null && state.State.endingId == endingId?.Trim();
}

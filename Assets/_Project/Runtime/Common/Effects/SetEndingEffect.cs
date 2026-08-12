using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Effects/Set Ending")]
public sealed class SetEndingEffect : GameEffect
{
    [SerializeField]
    private string endingId;

    [SerializeField]
    private Condition[] conditions;

    public override void Apply(GameStateStore state)
    {
        if (ConditionResolver.All(conditions, state))
            state?.TrySetEnding(endingId);
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Integrity")]
public sealed class IntegrityCondition : Condition
{
    [SerializeField]
    private int minimum;

    [SerializeField]
    private int maximum = 100;

    public override bool Evaluate(GameStateStore state) =>
        state != null
        && state.State.evidenceIntegrity >= minimum
        && state.State.evidenceIntegrity <= maximum;
}

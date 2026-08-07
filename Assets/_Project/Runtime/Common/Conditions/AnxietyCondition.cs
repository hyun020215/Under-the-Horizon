using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Anxiety")]
public sealed class AnxietyCondition : Condition
{
    [SerializeField]
    private int minimum;

    [SerializeField]
    private int maximum = 100;

    public override bool Evaluate(GameStateStore state) =>
        state != null
        && state.State.publicAnxiety >= minimum
        && state.State.publicAnxiety <= maximum;
}

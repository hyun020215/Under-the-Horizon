using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Interaction Completed")]
public sealed class InteractionCompletedCondition : Condition
{
    [SerializeField]
    private string interactionId;

    public override bool Evaluate(GameStateStore state) =>
        state != null && state.IsInteractionCompleted(interactionId);
}

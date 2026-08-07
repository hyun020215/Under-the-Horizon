using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Has Evidence")]
public sealed class HasEvidenceCondition : Condition
{
    [SerializeField] private string evidenceId;
    public override bool Evaluate(GameStateStore state) => state != null && state.HasEvidence(evidenceId);
}

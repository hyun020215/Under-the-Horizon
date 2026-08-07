using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Effects/Add Evidence")]
public sealed class AddEvidenceEffect : GameEffect
{
    [SerializeField]
    private string evidenceId;

    public override void Apply(GameStateStore state) => state?.AddEvidence(evidenceId);
}

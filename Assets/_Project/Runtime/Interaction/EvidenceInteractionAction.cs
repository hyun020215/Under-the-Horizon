using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Interaction Actions/Evidence")]
public sealed class EvidenceInteractionAction : InteractionAction
{
    [SerializeField]
    private string evidenceId;

    public override bool GrantsEvidence => !string.IsNullOrWhiteSpace(evidenceId);

    public override Task<InteractionResult> ExecuteAsync(InteractionContext context) =>
        Task.FromResult(
            new InteractionResult(context.State != null && context.State.AddEvidence(evidenceId))
        );
}

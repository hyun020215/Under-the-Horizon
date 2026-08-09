using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Interaction Actions/Investigation")]
public sealed class InvestigationInteractionAction : InteractionAction
{
    [SerializeField]
    private DialogueSequence dialogue;

    [SerializeField]
    private string startLineId;

    [SerializeField]
    private string endLineId;

    [SerializeField]
    private GameEffect[] effects;

    public override bool GrantsEvidence =>
        effects?.Any(effect => effect is AddEvidenceEffect) == true;

    public override async Task<InteractionResult> ExecuteAsync(InteractionContext context)
    {
        if (dialogue != null && context.Narrative != null)
            await context.Narrative.PlayAsync(dialogue, startLineId, endLineId);

        if (effects != null)
            foreach (var effect in effects)
                effect?.Apply(context.State);
        return InteractionResult.Completed;
    }
}

using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Interaction Actions/Dialogue")]
public sealed class DialogueInteractionAction : InteractionAction
{
    [SerializeField]
    private DialogueSequence dialogue;

    public override async Task<InteractionResult> ExecuteAsync(InteractionContext context)
    {
        if (context.Narrative == null || dialogue == null)
            return new(false);
        await context.Narrative.PlayAsync(dialogue);
        return InteractionResult.Completed;
    }
}

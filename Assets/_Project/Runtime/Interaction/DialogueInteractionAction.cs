using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Interaction Actions/Dialogue")]
public sealed class DialogueInteractionAction : InteractionAction
{
    [SerializeField]
    private DialogueSequence dialogue;

    [SerializeField]
    private string startLineId;

    [SerializeField]
    private string endLineId;

    [SerializeField]
    private bool advanceStorySceneOnComplete;

    public bool AdvanceStorySceneOnComplete => advanceStorySceneOnComplete;

    public override async Task<InteractionResult> ExecuteAsync(InteractionContext context)
    {
        if (context.Narrative == null || dialogue == null)
            return new(false);
        await context.Narrative.PlayAsync(dialogue, startLineId, endLineId);
        return advanceStorySceneOnComplete
            ? InteractionResult.CompletedWithStorySceneAdvance
            : InteractionResult.Completed;
    }
}

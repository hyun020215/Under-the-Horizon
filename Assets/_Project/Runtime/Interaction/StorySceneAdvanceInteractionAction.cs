using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Interaction Actions/Advance Story Scene")]
public sealed class StorySceneAdvanceInteractionAction : InteractionAction
{
    public override Task<InteractionResult> ExecuteAsync(InteractionContext context) =>
        Task.FromResult(InteractionResult.CompletedWithStorySceneAdvance);
}

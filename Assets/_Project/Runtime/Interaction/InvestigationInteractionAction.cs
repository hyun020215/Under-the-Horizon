using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Interaction Actions/Investigation")]
public sealed class InvestigationInteractionAction : InteractionAction
{
    [SerializeField]
    private GameEffect[] effects;

    public override Task<InteractionResult> ExecuteAsync(InteractionContext context)
    {
        if (effects != null)
            foreach (var effect in effects)
                effect?.Apply(context.State);
        return Task.FromResult(InteractionResult.Completed);
    }
}

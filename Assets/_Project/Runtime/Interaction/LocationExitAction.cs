using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Interaction Actions/Location Exit")]
public sealed class LocationExitAction : InteractionAction
{
    [SerializeField]
    private string locationId;

    public override Task<InteractionResult> ExecuteAsync(InteractionContext context)
    {
        context.State?.SetCurrentLocation(locationId);
        return Task.FromResult(InteractionResult.Completed);
    }
}

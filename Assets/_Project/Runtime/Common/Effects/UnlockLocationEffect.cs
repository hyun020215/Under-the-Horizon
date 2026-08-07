using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Effects/Unlock Location")]
public sealed class UnlockLocationEffect : GameEffect
{
    [SerializeField] private string locationId;
    public override void Apply(GameStateStore state) => state?.UnlockLocation(locationId);
}

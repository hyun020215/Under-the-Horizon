using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Effects/Change Integrity")]
public sealed class ChangeIntegrityEffect : GameEffect
{
    [SerializeField] private int amount;
    public override void Apply(GameStateStore state) => state?.ChangeIntegrity(amount);
}

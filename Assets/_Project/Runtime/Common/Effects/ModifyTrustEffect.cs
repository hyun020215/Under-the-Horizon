using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Effects/Modify Trust")]
public sealed class ModifyTrustEffect : GameEffect
{
    [SerializeField] private string characterId;
    [SerializeField] private int amount;
    public override void Apply(GameStateStore state) => state?.ModifyTrust(characterId, amount);
}

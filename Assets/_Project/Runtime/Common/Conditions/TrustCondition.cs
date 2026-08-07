using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Trust")]
public sealed class TrustCondition : Condition
{
    [SerializeField]
    private string characterId;

    [SerializeField]
    private int minimum;

    [SerializeField]
    private int maximum = 100;

    public override bool Evaluate(GameStateStore state) =>
        state != null
        && state.GetTrust(characterId) >= minimum
        && state.GetTrust(characterId) <= maximum;
}

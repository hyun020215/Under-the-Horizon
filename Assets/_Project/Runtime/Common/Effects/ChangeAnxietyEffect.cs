using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Effects/Change Anxiety")]
public sealed class ChangeAnxietyEffect : GameEffect
{
    [SerializeField] private int amount;
    public override void Apply(GameStateStore state) => state?.ChangeAnxiety(amount);
}

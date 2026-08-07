using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Effects/Composite")]
public sealed class CompositeEffect : GameEffect
{
    [SerializeField] private GameEffect[] effects;
    public override void Apply(GameStateStore state)
    {
        if (effects == null) return;
        foreach (GameEffect effect in effects) effect?.Apply(state);
    }
}

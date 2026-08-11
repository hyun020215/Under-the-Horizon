using UnityEngine;

[CreateAssetMenu(menuName = "Under The Horizon/Effects/Resolve Theory")]
public sealed class ResolveTheoryEffect : GameEffect
{
    [SerializeField]
    private string theoryId;

    public string TheoryId => theoryId;

    public override void Apply(GameStateStore state) => state?.ResolveTheory(theoryId);
}

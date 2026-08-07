using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Effects/Complete Objective")]
public sealed class CompleteObjectiveEffect : GameEffect
{
    [SerializeField] private string objectiveId;
    public override void Apply(GameStateStore state) => state?.CompleteObjective(objectiveId);
}

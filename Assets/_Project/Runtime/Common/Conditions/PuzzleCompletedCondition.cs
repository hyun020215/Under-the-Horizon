using UnityEngine;
[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Puzzle Completed")]
public sealed class PuzzleCompletedCondition : Condition
{
    [SerializeField] private string puzzleId;
    public override bool Evaluate(GameStateStore state) => state != null && state.IsPuzzleCompleted(puzzleId);
}

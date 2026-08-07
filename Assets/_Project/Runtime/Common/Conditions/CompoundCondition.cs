using UnityEngine;
public enum ConditionGroupMode { All, Any, Not }
[CreateAssetMenu(menuName = "Under The Horizon/Conditions/Compound")]
public sealed class CompoundCondition : Condition
{
    [SerializeField] private ConditionGroupMode mode;
    [SerializeField] private Condition[] conditions;
    public override bool Evaluate(GameStateStore state)
    {
        if (mode == ConditionGroupMode.Not) return conditions == null || conditions.Length == 0 || conditions[0] == null || !conditions[0].Evaluate(state);
        if (conditions == null || conditions.Length == 0) return mode == ConditionGroupMode.All;
        foreach (Condition condition in conditions)
        {
            bool result = condition == null || condition.Evaluate(state);
            if (mode == ConditionGroupMode.All && !result) return false;
            if (mode == ConditionGroupMode.Any && result) return true;
        }
        return mode == ConditionGroupMode.All;
    }
}

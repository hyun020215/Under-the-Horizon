using System.Collections.Generic;

public static class ConditionResolver
{
    public static bool All(IEnumerable<Condition> conditions, GameStateStore state)
    {
        if (conditions == null) return true;
        foreach (Condition condition in conditions)
            if (condition != null && !condition.Evaluate(state)) return false;
        return true;
    }
}

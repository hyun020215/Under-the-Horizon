using System.Collections.Generic;
using System;

public sealed class CargoRailPuzzleController : ValidatedPuzzleController
{
    private readonly List<string> route = new();

    protected override void ResetPuzzle() => route.Clear();

    public void SetRoute(IEnumerable<string> nodes)
    {
        route.Clear();
        if (nodes != null)
            route.AddRange(nodes);
    }

    public bool Submit(string start, string end) =>
        CompleteWhen(
            route.Count >= 2 && route[0] == start && route[^1] == end,
            string.Join(",", route)
        );

    public bool SubmitAuthoredRule()
    {
        PuzzleRuleDefinition rules = Context.Definition?.Rules;
        if (rules == null || rules.SolutionIds.Length == 0 || !HasRequiredEvidence())
            return false;
        if (rules.OrderMatters)
        {
            if (route.Count != rules.SolutionIds.Length)
                return false;
            for (int i = 0; i < route.Count; i++)
                if (!string.Equals(route[i], rules.SolutionIds[i], StringComparison.Ordinal))
                    return false;
        }
        else
        {
            foreach (string id in rules.SolutionIds)
                if (!route.Contains(id))
                    return false;
        }
        return CompleteWhen(true, string.Join(",", route));
    }
}

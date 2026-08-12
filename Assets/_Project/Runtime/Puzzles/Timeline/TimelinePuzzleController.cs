using System.Collections.Generic;
using System;

public sealed class TimelinePuzzleController : ValidatedPuzzleController
{
    private readonly List<string> cards = new();

    protected override void ResetPuzzle() => cards.Clear();

    public void SetOrder(IEnumerable<string> ids)
    {
        cards.Clear();
        if (ids != null)
            cards.AddRange(ids);
        if (Context.Definition != null)
        {
            string progress = string.Empty;
            Context.State?.TryGetPuzzleProgress(Context.Definition.Id, out progress);
            int hintMarker = progress?.IndexOf("|hint:", StringComparison.Ordinal) ?? -1;
            string hint = hintMarker >= 0 ? progress.Substring(hintMarker) : string.Empty;
            Context.State?.SetPuzzleProgress(
                Context.Definition.Id,
                string.Join(",", cards) + hint);
        }
    }

    public bool Submit(string[] expected)
    {
        if (expected == null || cards.Count != expected.Length)
            return false;
        for (int i = 0; i < expected.Length; i++)
            if (cards[i] != expected[i])
                return false;
        return CompleteWhen(true, string.Join(",", cards));
    }

    public bool SubmitAuthoredRule()
    {
        PuzzleRuleDefinition rules = Context.Definition?.Rules;
        if (rules?.IsAuthored != true || !HasRequiredEvidence())
            return false;
        if (rules.OrderMatters)
        {
            if (cards.Count != rules.SolutionIds.Length)
                return false;
            for (int i = 0; i < cards.Count; i++)
                if (!string.Equals(cards[i], rules.SolutionIds[i], StringComparison.Ordinal))
                    return false;
        }
        else
        {
            foreach (string id in rules.SolutionIds)
                if (!cards.Contains(id))
                    return false;
        }
        return CompleteWhen(true, string.Join(",", cards));
    }
}

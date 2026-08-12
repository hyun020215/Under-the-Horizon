using System.Collections.Generic;
using System;

public sealed class CCTVLogPuzzleController : ValidatedPuzzleController
{
    private readonly HashSet<string> observations = new();

    protected override void ResetPuzzle() => observations.Clear();

    public void Observe(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;
        string[] allowed = Context.Definition?.Rules?.AllowedInputIds;
        if (allowed == null || allowed.Length == 0 || Array.IndexOf(allowed, id) >= 0)
        {
            observations.Add(id);
            Context.State?.SetPuzzleProgress(
                Context.Definition.Id,
                string.Join(",", observations));
        }
    }

    public bool Submit()
    {
        PuzzleRuleDefinition rules = Context.Definition?.Rules;
        if (rules?.IsAuthored == true)
        {
            if (!HasRequiredEvidence())
                return false;
            foreach (string id in rules.SolutionIds)
                if (!observations.Contains(id))
                    return false;
            return CompleteWhen(true, string.Join(",", observations));
        }
        return CompleteWhen(
            observations.Contains("cctv")
            && observations.Contains("door_log")
            && observations.Contains("detector_error")
            && observations.Contains("location"),
            string.Join(",", observations));
    }
}

using System.Collections.Generic;

public sealed class LuminolPuzzleController : ValidatedPuzzleController
{
    private readonly HashSet<string> found = new();

    protected override void ResetPuzzle() => found.Clear();

    public void Inspect(string id, bool reacts)
    {
        if (reacts)
            found.Add(id);
    }

    public bool Submit(string[] required)
    {
        if (required != null)
            foreach (string id in required)
                if (!found.Contains(id))
                    return false;
        return CompleteWhen(true, string.Join(",", found));
    }
}

using System.Collections.Generic;

public sealed class ClaireContradictionPuzzleController : ValidatedPuzzleController
{
    private readonly HashSet<string> links = new();

    protected override void ResetPuzzle() => links.Clear();

    public void Link(string statement, string evidence) => links.Add(statement + ":" + evidence);

    public bool Submit(string requiredLink) =>
        CompleteWhen(links.Contains(requiredLink), requiredLink);
}

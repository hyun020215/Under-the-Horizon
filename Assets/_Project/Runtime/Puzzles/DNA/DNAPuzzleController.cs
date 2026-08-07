using System.Collections.Generic;

public sealed class DNAPuzzleController : ValidatedPuzzleController
{
    private readonly Dictionary<string, string> markers = new();

    protected override void ResetPuzzle() => markers.Clear();

    public void Match(string marker, string allele) => markers[marker] = allele;

    public bool Submit(IDictionary<string, string> expected)
    {
        if (expected == null)
            return false;
        foreach (var pair in expected)
            if (!markers.TryGetValue(pair.Key, out string value) || value != pair.Value)
                return false;
        return CompleteWhen(true, "dna_match");
    }
}

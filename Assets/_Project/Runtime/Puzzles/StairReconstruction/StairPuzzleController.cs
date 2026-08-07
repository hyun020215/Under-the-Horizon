using System.Collections.Generic;

public sealed class StairPuzzleController : ValidatedPuzzleController
{
    private readonly List<string> order = new();

    protected override void ResetPuzzle() => order.Clear();

    public void SetOrder(IEnumerable<string> ids)
    {
        order.Clear();
        if (ids != null)
            order.AddRange(ids);
    }

    public bool Submit(string[] expected)
    {
        if (expected == null || expected.Length != order.Count)
            return false;
        for (int i = 0; i < expected.Length; i++)
            if (expected[i] != order[i])
                return false;
        return CompleteWhen(true, string.Join(",", order));
    }
}

using System.Collections.Generic;

public sealed class DialogueHistory
{
    private readonly List<string> lines = new();
    public IReadOnlyList<string> Lines => lines;

    public void Add(string lineId)
    {
        if (!string.IsNullOrWhiteSpace(lineId))
            lines.Add(lineId);
    }

    public void Clear() => lines.Clear();
}

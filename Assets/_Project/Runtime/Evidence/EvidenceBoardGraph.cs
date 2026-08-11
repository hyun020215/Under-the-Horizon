using System;
using System.Collections.Generic;
using System.Linq;

public sealed class EvidenceBoardGraph
{
    private readonly HashSet<EvidenceDefinition> connected = new();
    public IReadOnlyCollection<EvidenceDefinition> Connected => connected;

    public bool Toggle(EvidenceDefinition evidence)
    {
        if (evidence == null)
            return false;
        if (!connected.Add(evidence))
            connected.Remove(evidence);
        return connected.Contains(evidence);
    }

    public void Clear() => connected.Clear();

    public bool Matches(TheoryDefinition theory)
    {
        if (theory == null)
            return false;
        EvidenceDefinition[] required = (theory.RequiredEvidence ?? Array.Empty<EvidenceDefinition>())
            .Where(item => item != null)
            .Distinct()
            .ToArray();
        return connected.Count == required.Length && required.All(connected.Contains);
    }
}

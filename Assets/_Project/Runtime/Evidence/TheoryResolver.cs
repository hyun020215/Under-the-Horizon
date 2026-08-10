using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct TheoryEvaluation
{
    public TheoryEvaluation(TheoryDefinition theory, IReadOnlyList<EvidenceDefinition> missingEvidence)
    {
        Theory = theory;
        MissingEvidence = missingEvidence ?? Array.Empty<EvidenceDefinition>();
    }

    public TheoryDefinition Theory { get; }
    public IReadOnlyList<EvidenceDefinition> MissingEvidence { get; }
    public bool CanResolve => Theory != null && MissingEvidence.Count == 0;
}

public sealed class TheoryResolver
{
    private readonly EvidenceInventory inventory;

    public TheoryResolver(EvidenceInventory inventory)
    {
        this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
    }

    public TheoryEvaluation Evaluate(TheoryDefinition theory)
    {
        if (theory == null)
            return new TheoryEvaluation(null, Array.Empty<EvidenceDefinition>());

        EvidenceDefinition[] missing = (theory.RequiredEvidence ?? Array.Empty<EvidenceDefinition>())
            .Where(item => item != null && !inventory.Has(item.Id))
            .ToArray();
        return new TheoryEvaluation(theory, missing);
    }
}

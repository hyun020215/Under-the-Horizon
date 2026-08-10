using System;
using System.Linq;
using UnityEngine;

public sealed class EvidenceBoardDirector : MonoBehaviour
{
    [SerializeField] private EvidenceDirector evidence;
    [SerializeField] private TheoryDefinition[] theories;
    private TheoryResolver resolver;

    public EvidenceDefinition[] Discovered => evidence?.Inventory?.Discovered
        .Where(item => item != null)
        .OrderBy(item => item.Id, StringComparer.Ordinal)
        .ToArray() ?? Array.Empty<EvidenceDefinition>();

    public TheoryEvaluation[] EvaluateTheories()
    {
        if (evidence?.Inventory == null)
            return Array.Empty<TheoryEvaluation>();
        resolver ??= new TheoryResolver(evidence.Inventory);
        return (theories ?? Array.Empty<TheoryDefinition>())
            .Where(item => item != null)
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .Select(resolver.Evaluate)
            .ToArray();
    }
}

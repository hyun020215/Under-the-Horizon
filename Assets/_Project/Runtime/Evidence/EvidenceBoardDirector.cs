using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public sealed class EvidenceBoardDirector : MonoBehaviour
{
    [SerializeField] private EvidenceDirector evidence;
    [SerializeField] private TheoryDefinition[] theories;
    private GameStateStore state;
    private TheoryResolver resolver;
    private readonly HashSet<string> announcedReadyTheories = new(StringComparer.Ordinal);
    public event Action<TheoryDefinition> TheoryReady;

    private void OnEnable()
    {
        if (evidence != null)
            evidence.EvidenceDiscovered += OnEvidenceDiscovered;
    }

    private void OnDisable()
    {
        if (evidence != null)
            evidence.EvidenceDiscovered -= OnEvidenceDiscovered;
    }

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
            .Select(item =>
            {
                TheoryEvaluation evaluation = resolver.Evaluate(item);
                return new TheoryEvaluation(
                    item,
                    evaluation.MissingEvidence,
                    State?.IsTheoryResolved(item.Id) == true);
            })
            .ToArray();
    }

    public bool TryResolve(TheoryDefinition theory, EvidenceBoardGraph graph)
    {
        if (theory == null || State == null || State.IsTheoryResolved(theory.Id)
            || evidence?.Inventory == null || graph?.Matches(theory) != true)
            return false;
        resolver ??= new TheoryResolver(evidence.Inventory);
        if (!resolver.Evaluate(theory).CanResolve)
            return false;
        foreach (GameEffect effect in theory.OnResolvedEffects ?? Array.Empty<GameEffect>())
            effect?.Apply(State);
        return State.IsTheoryResolved(theory.Id);
    }

    private GameStateStore State
    {
        get
        {
            if (state == null)
                AppContext.Services?.TryGet(out state);
            return state;
        }
    }

    private void OnEvidenceDiscovered(EvidenceDefinition discovered)
    {
        if (discovered == null)
            return;
        foreach (TheoryEvaluation evaluation in EvaluateTheories())
        {
            TheoryDefinition theory = evaluation.Theory;
            if (!evaluation.CanResolve || theory == null ||
                !theory.RequiredEvidence.Contains(discovered) ||
                !announcedReadyTheories.Add(theory.Id))
                continue;
            TheoryReady?.Invoke(theory);
        }
    }
}

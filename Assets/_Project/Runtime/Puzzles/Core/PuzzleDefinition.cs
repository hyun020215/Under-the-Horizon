using UnityEngine;
using System;

[Serializable]
public sealed class PuzzleRuleDefinition
{
    [SerializeField] private string[] allowedInputIds;
    [SerializeField] private string[] solutionIds;
    [SerializeField] private string[] requiredEvidenceIds;
    [SerializeField] private string[] hints;
    [SerializeField] private bool orderMatters;

    public string[] AllowedInputIds => allowedInputIds ?? Array.Empty<string>();
    public string[] SolutionIds => solutionIds ?? Array.Empty<string>();
    public string[] RequiredEvidenceIds => requiredEvidenceIds ?? Array.Empty<string>();
    public string[] Hints => hints ?? Array.Empty<string>();
    public bool OrderMatters => orderMatters;
    public bool IsAuthored =>
        AllowedInputIds.Length > 0
        || SolutionIds.Length > 0
        || RequiredEvidenceIds.Length > 0
        || Hints.Length > 0;
}

[CreateAssetMenu(fileName = "PUZ_", menuName = "Under The Horizon/Puzzles/Definition")]
public sealed class PuzzleDefinition : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private string controllerKey;

    [SerializeField]
    private GameEffect[] completionEffects;

    [SerializeField]
    private PuzzleRuleDefinition rules;
    public string Id => id;
    public string ControllerKey => controllerKey;
    public GameEffect[] CompletionEffects => completionEffects;
    public PuzzleRuleDefinition Rules => rules;

    public void ApplyCompletion(GameStateStore state)
    {
        state?.CompletePuzzle(id);
        if (completionEffects != null)
            foreach (var effect in completionEffects)
                effect?.Apply(state);
    }
}

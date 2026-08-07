using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class InteractionDirector : MonoBehaviour
{
    [SerializeField]
    private GameStateStore state;

    [SerializeField]
    private NarrativeDirector narrative;

    [SerializeField]
    private PuzzleDirector puzzles;
    public InteractionSet Current { get; private set; }

    public void Apply(InteractionSet set) => Current = set;

    public async Task<InteractionResult> ExecuteAsync(InteractionDefinition definition)
    {
        if (definition == null || !definition.IsAvailable(state) || definition.Action == null)
            return new InteractionResult(false, "Unavailable");
        InteractionResult result = await definition.Action.ExecuteAsync(
            new InteractionContext(state, narrative, puzzles)
        );
        if (result.Success && !definition.Repeatable)
            state?.CompleteInteraction(definition.Id);
        return result;
    }
}

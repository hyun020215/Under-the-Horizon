using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class PuzzleDirector : MonoBehaviour
{
    [SerializeField]
    private PuzzleControllerBase[] controllers;

    [SerializeField]
    private ScreenRouter screens;

    public async Task<PuzzleResult> PlayAsync(PuzzleDefinition definition, GameStateStore state)
    {
        if (definition == null)
            return PuzzleResult.Cancelled;
        if (screens != null)
            await screens.OpenAsync(ScreenId.Puzzle, new ScreenContext(definition));
        PuzzleControllerBase controller = Array.Find(
            controllers,
            item => item != null && item.ControllerKey == definition.ControllerKey
        );
        if (controller == null)
            throw new InvalidOperationException(
                $"Puzzle controller '{definition.ControllerKey}' is not registered."
            );
        PuzzleResult result = await controller.PlayAsync(new PuzzleContext(definition, state));
        if (result.Completed)
            definition.ApplyCompletion(state);
        return result;
    }
}

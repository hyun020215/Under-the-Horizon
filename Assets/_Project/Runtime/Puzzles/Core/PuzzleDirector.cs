using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class PuzzleDirector : MonoBehaviour
{
    [SerializeField]
    private PuzzleControllerBase[] controllers;

    [SerializeField]
    private ScreenRouter screens;
    [SerializeField] private PuzzleScreen puzzleScreen;
    private ValidatedPuzzleController activeController;

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
        activeController = controller as ValidatedPuzzleController;
        puzzleScreen?.Present(definition, CancelActive);
        PuzzleResult result = await controller.PlayAsync(new PuzzleContext(definition, state));
        activeController = null;
        if (result.Completed)
            definition.ApplyCompletion(state);
        puzzleScreen?.ShowResult(result);
        return result;
    }

    private void CancelActive() => activeController?.Cancel();
}

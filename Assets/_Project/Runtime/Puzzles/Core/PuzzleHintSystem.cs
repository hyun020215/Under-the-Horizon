using System;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ValidatedPuzzleController : PuzzleControllerBase
{
    private TaskCompletionSource<PuzzleResult> completion;
    protected PuzzleContext Context { get; private set; }

    public override Task<PuzzleResult> PlayAsync(PuzzleContext context)
    {
        Context = context;
        completion = new TaskCompletionSource<PuzzleResult>();
        ResetPuzzle();
        return completion.Task;
    }

    protected abstract void ResetPuzzle();

    protected bool CompleteWhen(bool valid, string payload = null)
    {
        if (!valid)
            return false;
        completion?.TrySetResult(new PuzzleResult(true, payload));
        return true;
    }

    public void Cancel() => completion?.TrySetResult(PuzzleResult.Cancelled);
}

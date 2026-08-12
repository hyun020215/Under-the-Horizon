using System;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ValidatedPuzzleController : PuzzleControllerBase
{
    private TaskCompletionSource<PuzzleResult> completion;
    protected PuzzleContext Context { get; private set; }
    public int HintLevel { get; private set; }

    public override Task<PuzzleResult> PlayAsync(PuzzleContext context)
    {
        Context = context;
        completion = new TaskCompletionSource<PuzzleResult>();
        HintLevel = 0;
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

    public string RequestHint()
    {
        string[] hints = Context.Definition?.Rules?.Hints;
        if (hints == null || HintLevel >= hints.Length)
            return string.Empty;
        string hint = hints[HintLevel++];
        Context.State?.SetPuzzleProgress(
            Context.Definition.Id,
            $"hint:{HintLevel}");
        return hint;
    }

    protected bool HasRequiredEvidence()
    {
        string[] required = Context.Definition?.Rules?.RequiredEvidenceIds;
        if (required == null)
            return true;
        foreach (string id in required)
            if (!Context.State.HasEvidence(id))
                return false;
        return true;
    }
}

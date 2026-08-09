using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public sealed class InputLockCommand : SequenceCommand
{
    [SerializeField]
    private bool locked;

    public override Task ExecuteAsync(SequenceContext context)
    {
        context.InputBlocker?.SetBlocked(locked);
        return Task.CompletedTask;
    }
}

using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public sealed class WaitCommand : SequenceCommand
{
    [SerializeField, Min(0)]
    private float seconds;

    public override Task ExecuteAsync(SequenceContext context) =>
        Task.Delay(Mathf.CeilToInt(seconds * 1000));
}

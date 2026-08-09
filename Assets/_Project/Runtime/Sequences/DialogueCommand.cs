using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public sealed class DialogueCommand : SequenceCommand
{
    [SerializeField]
    private DialogueSequence dialogue;

    [SerializeField]
    private string startLineId;

    [SerializeField]
    private string endLineId;

    public override Task ExecuteAsync(SequenceContext context) =>
        context.Narrative?.PlayAsync(dialogue, startLineId, endLineId)
        ?? Task.CompletedTask;
}

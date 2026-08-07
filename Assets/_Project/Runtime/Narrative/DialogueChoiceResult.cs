public readonly struct DialogueChoiceResult
{
    public DialogueChoiceResult(DialogueChoice choice)
    {
        Choice = choice;
    }

    public DialogueChoice Choice { get; }
}

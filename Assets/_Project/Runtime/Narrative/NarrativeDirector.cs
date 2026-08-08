using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class NarrativeDirector : MonoBehaviour
{
    [SerializeField]
    private GameStateStore state;

    [SerializeField]
    private ScreenRouter screens;

    [SerializeField]
    private VoiceController voice;

    [SerializeField]
    private TransitionDirector transitions;

    [SerializeField]
    private TransitionProfile dialogueOpenTransition;

    [SerializeField]
    private TransitionProfile dialogueCloseTransition;
    private readonly DialogueHistory history = new();
    public event Func<DialogueLine, Task<DialogueChoice>> LinePresented;
    public DialogueHistory History => history;
    public GameStateStore State => state;

    public async Task PlayAsync(DialogueSequence sequence)
    {
        if (sequence == null)
            return;
        ScreenId returnScreen = screens?.Current ?? ScreenId.Exploration;
        if (returnScreen == ScreenId.Dialogue)
            returnScreen = ScreenId.Exploration;
        if (screens != null)
        {
            await screens.OpenAsync(
                ScreenId.Dialogue,
                new ScreenContext(sequence),
                transitions,
                dialogueOpenTransition
            );
        }
        if (sequence.Lines == null)
            return;
        bool cancelled = false;
        foreach (DialogueLine line in sequence.Lines)
        {
            if (!ConditionResolver.All(line.conditions, state))
                continue;
            history.Add(line.id);
            if (line.voiceClip != null)
                voice?.Play(line.voiceClip);
            DialogueChoice selected = null;
            if (LinePresented != null)
            {
                try
                {
                    selected = await LinePresented.Invoke(line);
                }
                catch (TaskCanceledException)
                {
                    cancelled = true;
                    break;
                }
            }
            else
                selected = FirstAvailable(line.choices);
            if (line.effects != null)
                foreach (GameEffect effect in line.effects)
                    effect?.Apply(state);
            if (selected != null)
                selected.Apply(state);
        }
        if (!cancelled && screens != null)
        {
            await screens.OpenAsync(
                returnScreen,
                default,
                transitions,
                dialogueCloseTransition
            );
        }
    }

    private DialogueChoice FirstAvailable(IEnumerable<DialogueChoice> choices)
    {
        if (choices != null)
            foreach (var choice in choices)
                if (choice != null && choice.IsAvailable(state))
                    return choice;
        return null;
    }
}

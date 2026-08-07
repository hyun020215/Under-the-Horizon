using System;
using UnityEngine;

[Serializable]
public struct DialogueLine
{
    public string id;
    public DialogueSpeaker speaker;

    [TextArea]
    public string text;
    public CharacterExpression expression;
    public bool voiceRequired;
    public AudioClip voiceClip;
    public Condition[] conditions;
    public GameEffect[] effects;
    public DialogueChoice[] choices;
}

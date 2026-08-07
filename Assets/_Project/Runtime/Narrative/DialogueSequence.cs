using UnityEngine;

[CreateAssetMenu(fileName = "DIA_", menuName = "Under The Horizon/Dialogue/Sequence")]
public sealed class DialogueSequence : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private DialogueLine[] lines;
    public string Id => id;
    public DialogueLine[] Lines => lines;
}

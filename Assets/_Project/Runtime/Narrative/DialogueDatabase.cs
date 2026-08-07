using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DATABASE_Dialogue", menuName = "Under The Horizon/Dialogue/Database")]
public sealed class DialogueDatabase : ScriptableObject
{
    [SerializeField]
    private DialogueSequence[] sequences;
    public IReadOnlyList<DialogueSequence> Sequences => sequences;
}

using UnityEngine;

[CreateAssetMenu(fileName = "SEQ_", menuName = "Under The Horizon/Sequences/Definition")]
public sealed class SceneSequenceDefinition : ScriptableObject
{
    [SerializeReference]
    private SequenceCommand[] commands;
    public SequenceCommand[] Commands => commands;
}

using UnityEngine;

[CreateAssetMenu(fileName = "THEORY_", menuName = "Under The Horizon/Evidence/Theory")]
public sealed class TheoryDefinition : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private string displayName;

    [TextArea, SerializeField]
    private string description;

    [SerializeField]
    private EvidenceDefinition[] requiredEvidence;

    [SerializeField]
    private GameEffect[] onResolvedEffects;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public EvidenceDefinition[] RequiredEvidence => requiredEvidence;
    public GameEffect[] OnResolvedEffects => onResolvedEffects;
}

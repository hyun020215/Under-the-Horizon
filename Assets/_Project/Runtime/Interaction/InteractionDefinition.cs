using UnityEngine;

[CreateAssetMenu(fileName = "INT_", menuName = "Under The Horizon/Interaction/Definition")]
public sealed class InteractionDefinition : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private InteractionType type;

    [SerializeField]
    private string displayName;

    [SerializeField]
    private string targetId;

    [SerializeField]
    private bool hasWorldHotspot;

    [SerializeField]
    private WorldMarkerVisibility worldMarkerVisibility =
        WorldMarkerVisibility.Always;

    [SerializeField]
    private Rect normalizedRect;

    [SerializeField]
    private Condition[] conditions;

    [SerializeField]
    private InteractionAction action;

    [SerializeField]
    private bool repeatable;
    public string Id => id;
    public InteractionType Type => type;
    public string DisplayName => displayName;
    public string TargetId => targetId;
    public bool HasWorldHotspot => hasWorldHotspot;
    public WorldMarkerVisibility WorldMarkerVisibility => worldMarkerVisibility;
    public Rect NormalizedRect => normalizedRect;
    public InteractionAction Action => action;
    public bool Repeatable => repeatable;

    public bool IsAvailable(GameStateStore state) =>
        ConditionResolver.All(conditions, state)
        && (repeatable || state == null || !state.IsInteractionCompleted(id));

    public bool MatchesTarget(string candidateId) =>
        string.IsNullOrWhiteSpace(targetId)
        || string.Equals(targetId, candidateId, System.StringComparison.Ordinal);
}

using UnityEngine;

[CreateAssetMenu(fileName = "C00_", menuName = "Under The Horizon/Evidence/Definition")]
public sealed class EvidenceDefinition : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private string displayName;

    [TextArea, SerializeField]
    private string description;

    [SerializeField]
    private Sprite image;

    [SerializeField]
    private EvidenceQuality quality;

    [SerializeField]
    private string category;

    [SerializeField]
    private bool isDirect;
    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Image => image;
    public EvidenceQuality Quality => quality;
    public string Category => category;
    public bool IsDirect => isDirect;
}

using UnityEngine;

[CreateAssetMenu(fileName = "MAP_", menuName = "Under The Horizon/Locations/Map")]
public sealed class MapDefinition : ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private string displayName;

    [SerializeField]
    private Sprite baseLayer;

    [SerializeField]
    private Sprite restrictedLayer;

    [SerializeField]
    private Sprite technicalLayer;

    [SerializeField]
    private LocationDefinition[] locations;

    public string Id => id;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName)
        ? "알 수 없는 지도"
        : displayName;
    public Sprite BaseLayer => baseLayer;
    public Sprite RestrictedLayer => restrictedLayer;
    public Sprite TechnicalLayer => technicalLayer;
    public LocationDefinition[] Locations => locations;
}

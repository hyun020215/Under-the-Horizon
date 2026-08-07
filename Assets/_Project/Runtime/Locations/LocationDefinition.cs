using UnityEngine;
[CreateAssetMenu(fileName="LOC_",menuName="Under The Horizon/Locations/Definition")]
public sealed class LocationDefinition:ScriptableObject
{
    [SerializeField]private string id;[SerializeField]private string displayName;[SerializeField]private Sprite defaultBackground;
    [SerializeField]private LocationStateDefinition[] states;[SerializeField]private LocationExit[] exits;[SerializeField]private AudioCueProfile defaultAudio;[SerializeField]private MapNodeDefinition mapNode;
    public string Id=>id;public string DisplayName=>displayName;public Sprite DefaultBackground=>defaultBackground;public LocationStateDefinition[] States=>states;public LocationExit[] Exits=>exits;public AudioCueProfile DefaultAudio=>defaultAudio;public MapNodeDefinition MapNode=>mapNode;
}

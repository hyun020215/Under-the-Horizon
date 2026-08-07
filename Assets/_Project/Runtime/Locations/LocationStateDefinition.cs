using UnityEngine;
[CreateAssetMenu(fileName="LOCATION_STATE",menuName="Under The Horizon/Locations/State")]
public sealed class LocationStateDefinition:ScriptableObject { [SerializeField]private string id;[SerializeField]private Sprite background;[SerializeField]private Color tint=Color.white;[SerializeField]private AudioCueProfile audioOverride; public string Id=>id;public Sprite Background=>background;public Color Tint=>tint;public AudioCueProfile AudioOverride=>audioOverride; }

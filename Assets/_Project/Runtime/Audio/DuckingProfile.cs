using UnityEngine;

[CreateAssetMenu(fileName = "DUCK_", menuName = "Under The Horizon/Audio/Ducking")]
public sealed class DuckingProfile : ScriptableObject
{
    [Range(0, 1)]
    public float musicMultiplier = .45f;

    [Range(0, 1)]
    public float ambienceMultiplier = .65f;

    [Min(0)]
    public float attack = .15f;

    [Min(0)]
    public float release = .3f;
}

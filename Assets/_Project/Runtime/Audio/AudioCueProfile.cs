using UnityEngine;

[CreateAssetMenu(fileName="AUDIO_", menuName="Under The Horizon/Audio/Cue Profile")]
public sealed class AudioCueProfile : ScriptableObject
{
    public AudioClip music;

    public AudioClip ambienceA;
    public AudioClip ambienceB;

    public float musicVolume;
    public float ambienceAVolume;
    public float ambienceBVolume;

    public float crossfadeDuration;

    public AudioClip entryStinger;

    public DuckingProfile dialogueDucking;
}

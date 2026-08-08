using UnityEngine;

public sealed class AudioDirector : MonoBehaviour
{
    [SerializeField]
    private MusicController music;

    [SerializeField]
    private AmbienceController ambience;

    [SerializeField]
    private SfxController sfx;
    public AudioCueProfile Current { get; private set; }

    public void Apply(AudioCueProfile profile)
    {
        if (profile == null)
            return;
        Current = profile;
        music?.Play(profile.music, profile.musicVolume);
        ambience?.Apply(
            profile.ambienceA,
            profile.ambienceAVolume,
            profile.ambienceB,
            profile.ambienceBVolume
        );
        if (profile.entryStinger != null)
            sfx?.Play(profile.entryStinger);
    }

    public void SetMasterVolume(float value) => AudioListener.volume = Mathf.Clamp01(value);

    public void SetMusicVolume(float value) =>
        music?.SetVolume((Current?.musicVolume ?? 1f) * Mathf.Clamp01(value));

    public void SetSfxVolume(float value) => sfx?.SetVolume(value);
}

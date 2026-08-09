using UnityEngine;

public sealed class AudioDirector : MonoBehaviour
{
    [SerializeField]
    private MusicController music;

    [SerializeField]
    private AmbienceController ambience;

    [SerializeField]
    private SfxController sfx;

    [SerializeField]
    private VoiceController voice;

    [SerializeField]
    private AudioDuckingController ducking;
    public AudioCueProfile Current { get; private set; }

    public void Apply(AudioCueProfile profile)
    {
        if (profile == null)
            return;
        Current = profile;
        music?.Play(
            profile.music,
            profile.musicVolume,
            profile.crossfadeDuration);
        ambience?.Apply(
            profile.ambienceA,
            profile.ambienceAVolume,
            profile.ambienceB,
            profile.ambienceBVolume
        );
        if (profile.entryStinger != null)
            sfx?.Play(profile.entryStinger);
        ducking?.SetProfile(profile.dialogueDucking);
    }

    public void SetMasterVolume(float value) => AudioListener.volume = Mathf.Clamp01(value);

    public void SetMusicVolume(float value) => music?.SetVolume(value);

    public void SetSfxVolume(float value)
    {
        sfx?.SetVolume(value);
        voice?.SetVolume(value);
    }

    public void SetAmbienceVolume(float value) => ambience?.SetVolume(value);

    public void SetVoiceVolume(float value) => voice?.SetVolume(value);

    public void PlayVoiceBark(AudioClip clip) => voice?.PlayBark(clip);

    public void PlayStoryVoice(AudioClip clip) => voice?.PlayStory(clip);

    public void PlayCinematicStinger(AudioClip clip, float volume = 1f) =>
        sfx?.PlayExclusive(clip, volume);

    public System.Threading.Tasks.Task FadeOutCinematicStingerAsync(float duration) =>
        sfx?.FadeOutExclusiveAsync(duration)
        ?? System.Threading.Tasks.Task.CompletedTask;
}

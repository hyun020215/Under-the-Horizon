using UnityEngine;

public sealed class MusicController : MonoBehaviour
{
    [SerializeField]
    private AudioSource source;

    [SerializeField]
    private AudioCrossfade crossfade;

    private float profileVolume = 1f;
    private float userVolume = 1f;
    private float duckingMultiplier = 1f;

    private void Awake() => crossfade ??= GetComponent<AudioCrossfade>();

    public void Play(AudioClip clip, float volume)
        => Play(clip, volume, 0f);

    public void Play(AudioClip clip, float volume, float duration)
    {
        profileVolume = Mathf.Clamp01(volume);
        if (crossfade != null)
        {
            crossfade.Play(clip, EffectiveVolume, duration);
            return;
        }

        if (source == null)
            return;
        if (source.clip == clip && source.isPlaying)
        {
            source.volume = EffectiveVolume;
            return;
        }
        source.clip = clip;
        source.volume = EffectiveVolume;
        source.loop = true;
        if (clip != null)
            source.Play();
        else
            source.Stop();
    }

    public void SetVolume(float value)
    {
        userVolume = Mathf.Clamp01(value);
        crossfade?.SetVolume(EffectiveVolume);
        if (source != null)
            source.volume = EffectiveVolume;
    }

    public void SetDuckingMultiplier(float value)
    {
        duckingMultiplier = Mathf.Clamp01(value);
        crossfade?.SetVolume(EffectiveVolume);
        if (source != null)
            source.volume = EffectiveVolume;
    }

    private float EffectiveVolume => profileVolume * userVolume * duckingMultiplier;
}

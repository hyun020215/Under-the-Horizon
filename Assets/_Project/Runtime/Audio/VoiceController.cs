using UnityEngine;

public sealed class VoiceController : MonoBehaviour
{
    [SerializeField]
    private AudioSource source;

    [SerializeField]
    private AudioSource barkSource;

    private void Awake()
    {
        if (barkSource != null)
            return;
        barkSource = gameObject.AddComponent<AudioSource>();
        barkSource.playOnAwake = false;
        if (source != null)
            barkSource.outputAudioMixerGroup = source.outputAudioMixerGroup;
    }

    public void Play(AudioClip clip)
        => PlayStory(clip);

    public void PlayStory(AudioClip clip)
    {
        if (source == null)
            return;
        source.Stop();
        source.clip = clip;
        if (clip != null)
            source.Play();
    }

    public void PlayBark(AudioClip clip)
    {
        if (barkSource == null)
            Awake();
        if (barkSource != null && clip != null)
            barkSource.PlayOneShot(clip);
    }

    public void Stop() => source?.Stop();

    public void SetVolume(float value)
    {
        float volume = Mathf.Clamp01(value);
        if (source != null)
            source.volume = volume;
        if (barkSource != null)
            barkSource.volume = volume;
    }
}

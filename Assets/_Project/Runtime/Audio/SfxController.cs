using UnityEngine;

public sealed class SfxController : MonoBehaviour
{
    [SerializeField]
    private AudioSource source;

    [SerializeField]
    private AudioSource loopSource;

    public void Play(AudioClip clip, float volume = 1)
    {
        if (source != null && clip != null)
            source.PlayOneShot(clip, volume);
    }

    public void PlayLoop(AudioClip clip, float volume = 1f)
    {
        if (loopSource == null || clip == null)
            return;
        if (loopSource.clip == clip && loopSource.isPlaying)
            return;
        loopSource.clip = clip;
        loopSource.volume = volume;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StopLoop(AudioClip clip = null)
    {
        if (loopSource == null || (clip != null && loopSource.clip != clip))
            return;
        loopSource.Stop();
        loopSource.clip = null;
    }

    public void SetVolume(float value)
    {
        if (source != null)
            source.volume = Mathf.Clamp01(value);
        if (loopSource != null)
            loopSource.volume = Mathf.Clamp01(value);
    }
}

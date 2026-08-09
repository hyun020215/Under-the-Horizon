using System.Threading.Tasks;
using UnityEngine;

public sealed class SfxController : MonoBehaviour
{
    [SerializeField]
    private AudioSource source;

    [SerializeField]
    private AudioSource loopSource;

    private float userVolume = 1f;

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
        loopSource.volume = userVolume * Mathf.Clamp01(volume);
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
        userVolume = Mathf.Clamp01(value);
        if (source != null)
            source.volume = userVolume;
        if (loopSource != null)
            loopSource.volume = userVolume;
    }

    public void PlayExclusive(AudioClip clip, float volume = 1f)
    {
        if (loopSource == null || clip == null)
            return;
        loopSource.Stop();
        loopSource.clip = clip;
        loopSource.loop = false;
        loopSource.volume = userVolume * Mathf.Clamp01(volume);
        loopSource.Play();
    }

    public async Task FadeOutExclusiveAsync(float duration)
    {
        if (loopSource == null || !loopSource.isPlaying)
            return;

        float startVolume = loopSource.volume;
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0f, duration);
        while (elapsed < safeDuration && loopSource != null)
        {
            elapsed += Time.unscaledDeltaTime;
            loopSource.volume = Mathf.Lerp(
                startVolume,
                0f,
                Mathf.Clamp01(elapsed / safeDuration));
            await Task.Yield();
        }

        if (loopSource == null)
            return;
        loopSource.Stop();
        loopSource.clip = null;
        loopSource.volume = userVolume;
    }
}

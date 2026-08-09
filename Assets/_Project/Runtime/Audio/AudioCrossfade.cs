using System.Collections;
using UnityEngine;

public sealed class AudioCrossfade : MonoBehaviour
{
    [SerializeField]
    private AudioSource sourceA;

    [SerializeField]
    private AudioSource sourceB;

    private AudioSource active;
    private AudioSource inactive;
    private Coroutine fadeRoutine;
    private float targetVolume = 1f;

    public AudioSource ActiveSource => active;

    private void Awake()
    {
        sourceA ??= GetComponent<AudioSource>();
        if (sourceB == null)
        {
            sourceB = gameObject.AddComponent<AudioSource>();
            CopyRouting(sourceA, sourceB);
        }

        active = sourceA;
        inactive = sourceB;
    }

    public void Play(AudioClip clip, float volume, float duration)
    {
        EnsureInitialized();
        targetVolume = Mathf.Clamp01(volume);

        if (active != null && active.clip == clip && active.isPlaying)
        {
            active.volume = targetVolume;
            return;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(clip, Mathf.Max(0f, duration)));
    }

    public void SetVolume(float volume)
    {
        targetVolume = Mathf.Clamp01(volume);
        if (active != null)
            active.volume = targetVolume;
    }

    private IEnumerator FadeTo(AudioClip clip, float duration)
    {
        inactive.clip = clip;
        inactive.loop = true;
        inactive.volume = 0f;
        if (clip != null)
            inactive.Play();

        float previousVolume = active != null ? active.volume : 0f;
        if (duration <= 0f)
        {
            CompleteSwap();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            if (active != null)
                active.volume = Mathf.Lerp(previousVolume, 0f, progress);
            inactive.volume = Mathf.Lerp(0f, targetVolume, progress);
            yield return null;
        }

        CompleteSwap();
    }

    private void CompleteSwap()
    {
        if (active != null)
        {
            active.Stop();
            active.clip = null;
            active.volume = 0f;
        }

        inactive.volume = targetVolume;
        (active, inactive) = (inactive, active);
        fadeRoutine = null;
    }

    private void EnsureInitialized()
    {
        if (active == null || inactive == null)
            Awake();
    }

    private static void CopyRouting(AudioSource source, AudioSource target)
    {
        if (source == null || target == null)
            return;
        target.outputAudioMixerGroup = source.outputAudioMixerGroup;
        target.playOnAwake = false;
        target.spatialBlend = source.spatialBlend;
    }
}

using UnityEngine;

public sealed class AmbienceController : MonoBehaviour
{
    [SerializeField]
    private AudioSource sourceA;

    [SerializeField]
    private AudioSource sourceB;

    private float profileVolumeA;
    private float profileVolumeB;
    private float userVolume = 1f;
    private float duckingMultiplier = 1f;

    public void Apply(AudioClip a, float volumeA, AudioClip b, float volumeB)
    {
        profileVolumeA = Mathf.Clamp01(volumeA);
        profileVolumeB = Mathf.Clamp01(volumeB);
        Play(sourceA, a, EffectiveVolumeA);
        Play(sourceB, b, EffectiveVolumeB);
    }

    public void SetVolume(float value)
    {
        userVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetDuckingMultiplier(float value)
    {
        duckingMultiplier = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    private static void Play(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null)
            return;
        source.clip = clip;
        source.volume = volume;
        source.loop = true;
        if (clip != null)
            source.Play();
        else
            source.Stop();
    }

    private void ApplyVolumes()
    {
        if (sourceA != null)
            sourceA.volume = EffectiveVolumeA;
        if (sourceB != null)
            sourceB.volume = EffectiveVolumeB;
    }

    private float EffectiveVolumeA =>
        profileVolumeA * userVolume * duckingMultiplier;

    private float EffectiveVolumeB =>
        profileVolumeB * userVolume * duckingMultiplier;
}

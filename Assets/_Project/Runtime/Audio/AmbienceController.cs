using UnityEngine;

public sealed class AmbienceController : MonoBehaviour
{
    [SerializeField]
    private AudioSource sourceA;

    [SerializeField]
    private AudioSource sourceB;

    public void Apply(AudioClip a, float volumeA, AudioClip b, float volumeB)
    {
        Play(sourceA, a, volumeA);
        Play(sourceB, b, volumeB);
    }

    private static void Play(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null)
            return;
        source.clip = clip;
        source.volume = volume;
        if (clip != null)
            source.Play();
        else
            source.Stop();
    }
}

using UnityEngine;

public sealed class VoiceController : MonoBehaviour
{
    [SerializeField]
    private AudioSource source;

    public void Play(AudioClip clip)
    {
        if (source == null)
            return;
        source.Stop();
        source.clip = clip;
        if (clip != null)
            source.Play();
    }

    public void Stop() => source?.Stop();
}

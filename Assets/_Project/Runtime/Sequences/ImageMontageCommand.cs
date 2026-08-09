using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public sealed class ImageMontageCommand : SequenceCommand
{
    [SerializeField]
    private Texture2D[] frames;

    [SerializeField]
    private float[] holdSeconds;

    [SerializeField, Min(0f)]
    private float fadeInSeconds = 0.22f;

    [SerializeField, Min(0f)]
    private float betweenFadeSeconds = 0.16f;

    [SerializeField, Min(0f)]
    private float exitFadeSeconds = 0.5f;

    [SerializeField, Min(1f)]
    private float startScale = 1.035f;

    [SerializeField]
    private AudioClip stinger;

    [SerializeField, Range(0f, 1f)]
    private float stingerVolume = 0.88f;

    [SerializeField]
    private string seenFlag;

    public Texture2D[] Frames => frames;
    public float[] HoldSeconds => holdSeconds;
    public string SeenFlag => seenFlag;

    public override async Task ExecuteAsync(SequenceContext context)
    {
        if (!string.IsNullOrWhiteSpace(seenFlag)
            && context.State?.HasFlag(seenFlag) == true)
        {
            return;
        }

        if (context.CinematicOverlay == null)
        {
            throw new InvalidOperationException(
                "Sequence has no cinematic overlay presenter.");
        }

        context.Audio?.PlayCinematicStinger(stinger, stingerVolume);
        Task audioFade = Task.CompletedTask;
        await context.CinematicOverlay.PlayAsync(
            frames,
            holdSeconds,
            fadeInSeconds,
            betweenFadeSeconds,
            exitFadeSeconds,
            startScale,
            () => audioFade = context.Audio?.FadeOutCinematicStingerAsync(
                exitFadeSeconds) ?? Task.CompletedTask);
        await audioFade;

        if (!string.IsNullOrWhiteSpace(seenFlag))
            context.State?.SetFlag(seenFlag);
    }
}

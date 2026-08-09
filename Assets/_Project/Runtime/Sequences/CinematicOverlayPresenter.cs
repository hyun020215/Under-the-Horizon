using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CinematicOverlayPresenter : MonoBehaviour
{
    private CanvasGroup rootGroup;
    private RawImage frameImage;
    private RectTransform frameRect;

    public bool IsPlaying { get; private set; }
    public Texture CurrentFrame => frameImage != null ? frameImage.texture : null;

    private void Awake() => EnsureView();

    private void OnDisable()
    {
        IsPlaying = false;
        Hide();
    }

    public async Task PlayAsync(
        Texture2D[] frames,
        float[] holdSeconds,
        float fadeInSeconds,
        float betweenFadeSeconds,
        float exitFadeSeconds,
        float startScale,
        Action exitStarted = null)
    {
        EnsureView();
        if (rootGroup == null || frames == null || frames.Length == 0)
            return;

        IsPlaying = true;
        rootGroup.gameObject.SetActive(true);
        rootGroup.alpha = 1f;
        frameImage.color = TransparentWhite;

        try
        {
            for (var index = 0; index < frames.Length; index++)
            {
                if (frames[index] == null)
                    continue;
                await PresentFrameAsync(
                    frames[index],
                    HoldAt(holdSeconds, index),
                    fadeInSeconds,
                    startScale);
                if (index < frames.Length - 1)
                    await FadeFrameAsync(1f, 0f, betweenFadeSeconds);
            }

            exitStarted?.Invoke();
            await FadeRootAsync(1f, 0f, exitFadeSeconds);
        }
        finally
        {
            IsPlaying = false;
            Hide();
        }
    }

    private async Task PresentFrameAsync(
        Texture2D texture,
        float holdSeconds,
        float fadeSeconds,
        float startScale)
    {
        frameImage.texture = texture;
        AspectRatioFitter fitter = frameImage.GetComponent<AspectRatioFitter>();
        if (texture.height > 0)
            fitter.aspectRatio = (float)texture.width / texture.height;

        float scale = Mathf.Max(1f, startScale);
        frameRect.localScale = Vector3.one * scale;
        await FadeFrameAsync(0f, 1f, fadeSeconds);

        float elapsed = 0f;
        float duration = Mathf.Max(0f, holdSeconds);
        while (elapsed < duration && frameRect != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            frameRect.localScale = Vector3.one * Mathf.Lerp(scale, 1f, progress);
            await Task.Yield();
        }

        if (frameRect != null)
            frameRect.localScale = Vector3.one;
    }

    private async Task FadeFrameAsync(float from, float to, float duration)
    {
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0f, duration);
        while (elapsed < safeDuration && frameImage != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / safeDuration);
            frameImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(from, to, progress));
            await Task.Yield();
        }

        if (frameImage != null)
            frameImage.color = new Color(1f, 1f, 1f, to);
    }

    private async Task FadeRootAsync(float from, float to, float duration)
    {
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0f, duration);
        while (elapsed < safeDuration && rootGroup != null)
        {
            elapsed += Time.unscaledDeltaTime;
            rootGroup.alpha = Mathf.Lerp(
                from,
                to,
                Mathf.Clamp01(elapsed / safeDuration));
            await Task.Yield();
        }
    }

    private void EnsureView()
    {
        if (rootGroup != null)
            return;

        var root = new GameObject(
            "Cinematic Overlay",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasGroup),
            typeof(GraphicRaycaster),
            typeof(Image));
        root.transform.SetParent(transform, false);
        Stretch(root.GetComponent<RectTransform>());

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 175;

        Image backdrop = root.GetComponent<Image>();
        backdrop.color = Color.black;
        backdrop.raycastTarget = true;
        rootGroup = root.GetComponent<CanvasGroup>();
        rootGroup.interactable = true;
        rootGroup.blocksRaycasts = true;

        var frame = new GameObject(
            "Discovery Frame",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage),
            typeof(AspectRatioFitter));
        frame.transform.SetParent(root.transform, false);
        frameRect = frame.GetComponent<RectTransform>();
        Stretch(frameRect);
        frameImage = frame.GetComponent<RawImage>();
        frameImage.raycastTarget = false;
        AspectRatioFitter fitter = frame.GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = 16f / 9f;
        Hide();
    }

    private void Hide()
    {
        if (rootGroup == null)
            return;
        rootGroup.alpha = 0f;
        rootGroup.gameObject.SetActive(false);
        if (frameImage != null)
            frameImage.texture = null;
    }

    private static float HoldAt(float[] values, int index) =>
        values != null && index >= 0 && index < values.Length
            ? Mathf.Max(0f, values[index])
            : 1f;

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static readonly Color TransparentWhite = new(1f, 1f, 1f, 0f);
}

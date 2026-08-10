using System.Threading.Tasks;
using UnityEngine;

public abstract class ScreenBase : MonoBehaviour
{
    [SerializeField]
    private ScreenId id;
    private int transitionVersion;
    public ScreenId Id => id;

    public virtual Task OpenAsync(ScreenContext context)
    {
        gameObject.SetActive(true);
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = true;
        group.interactable = true;
        RectTransform rect = transform as RectTransform;
        Vector3 authoredScale = rect != null ? rect.localScale : Vector3.one;
        group.alpha = 0f;
        if (rect != null)
            rect.localScale = authoredScale * 0.985f;
        int version = ++transitionVersion;
        _ = Animate(group, rect, authoredScale, 0f, 1f, 0.18f, version);
        return Task.CompletedTask;
    }

    public virtual Task CloseAsync()
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        RectTransform rect = transform as RectTransform;
        Vector3 authoredScale = rect != null ? rect.localScale : Vector3.one;
        int version = ++transitionVersion;
        _ = CloseAfterAnimation(group, rect, authoredScale, version);
        return Task.CompletedTask;
    }

    private async Task CloseAfterAnimation(
        CanvasGroup group,
        RectTransform rect,
        Vector3 authoredScale,
        int version)
    {
        await Animate(group, rect, authoredScale * 0.99f,
            group != null ? group.alpha : 1f, 0f, 0.12f, version);
        if (version != transitionVersion || this == null)
            return;
        gameObject.SetActive(false);
        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = true;
        if (rect != null)
            rect.localScale = authoredScale;
    }

    private async Task Animate(
        CanvasGroup group,
        RectTransform rect,
        Vector3 targetScale,
        float fromAlpha,
        float toAlpha,
        float duration,
        int version)
    {
        float started = Time.realtimeSinceStartup;
        Vector3 startScale = rect != null ? rect.localScale : Vector3.one;
        while (Time.realtimeSinceStartup - started < duration)
        {
            if (version != transitionVersion || this == null || group == null)
                return;
            float t = Mathf.Clamp01((Time.realtimeSinceStartup - started) / duration);
            t = t * t * (3f - 2f * t);
            group.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            if (rect != null)
                rect.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
            await Task.Yield();
        }
        if (version != transitionVersion || this == null || group == null)
            return;
        group.alpha = toAlpha;
        if (rect != null)
            rect.localScale = targetScale;
    }
}

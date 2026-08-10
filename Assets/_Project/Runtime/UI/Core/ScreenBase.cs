using System.Threading.Tasks;
using UnityEngine;

public abstract class ScreenBase : MonoBehaviour
{
    [SerializeField]
    private ScreenId id;
    public ScreenId Id => id;

    public virtual async Task OpenAsync(ScreenContext context)
    {
        gameObject.SetActive(true);
        CanvasGroup group = GetComponent<CanvasGroup>()
            ?? gameObject.AddComponent<CanvasGroup>();
        RectTransform rect = transform as RectTransform;
        Vector3 authoredScale = rect != null ? rect.localScale : Vector3.one;
        group.alpha = 0f;
        if (rect != null)
            rect.localScale = authoredScale * 0.985f;
        await Animate(group, rect, authoredScale, 0f, 1f, 0.18f);
    }

    public virtual async Task CloseAsync()
    {
        CanvasGroup group = GetComponent<CanvasGroup>()
            ?? gameObject.AddComponent<CanvasGroup>();
        RectTransform rect = transform as RectTransform;
        Vector3 authoredScale = rect != null ? rect.localScale : Vector3.one;
        await Animate(group, rect, authoredScale * 0.99f, group.alpha, 0f, 0.12f);
        gameObject.SetActive(false);
        group.alpha = 1f;
        if (rect != null)
            rect.localScale = authoredScale;
    }

    private static async Task Animate(
        CanvasGroup group,
        RectTransform rect,
        Vector3 targetScale,
        float fromAlpha,
        float toAlpha,
        float duration)
    {
        float started = Time.realtimeSinceStartup;
        Vector3 startScale = rect != null ? rect.localScale : Vector3.one;
        while (Time.realtimeSinceStartup - started < duration)
        {
            float t = Mathf.Clamp01((Time.realtimeSinceStartup - started) / duration);
            t = t * t * (3f - 2f * t);
            group.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            if (rect != null)
                rect.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
            await Task.Yield();
        }
        group.alpha = toAlpha;
        if (rect != null)
            rect.localScale = targetScale;
    }
}

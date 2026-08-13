using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class FadeTransitionPlayer : TransitionPlayer
{
    [SerializeField]
    private CanvasGroup overlay;
    [SerializeField] private Image cover;
    private RectTransform[] particles;
    private Image[] particleImages;

    public override bool Supports(TransitionType type) =>
        type == TransitionType.Fade || type == TransitionType.Instant;

    public override async Task PlayAsync(TransitionRequest request)
    {
        if (overlay == null || request.Profile == null)
            return;
        cover ??= overlay.GetComponent<Image>();
        if (cover != null)
            cover.color = request.Profile.coverColor;
        BuildParticles(request.Profile);
        float duration = request.Entering
            ? request.Profile.coverDuration
            : request.Profile.revealDuration;
        float from = request.Entering ? 0 : 1,
            to = request.Entering ? 1 : 0;
        if (request.Profile.type == TransitionType.Instant || request.ReducedMotion)
            duration = 0;
        float elapsed = 0;
        do
        {
            elapsed += Time.unscaledDeltaTime;
            overlay.alpha = Mathf.Lerp(from, to, duration <= 0 ? 1 : elapsed / duration);
            UpdateParticles(request.Profile, request.Entering, duration <= 0 ? 1 : elapsed / duration,
                request.ReducedMotion);
            await Task.Yield();
        } while (elapsed < duration);
        overlay.alpha = to;
        UpdateParticles(request.Profile, request.Entering, 1f, request.ReducedMotion);
    }

    private void BuildParticles(TransitionProfile profile)
    {
        int count = Mathf.Max(0, profile.particleCount);
        if (particles != null && particles.Length == count)
            return;
        if (particles != null)
            foreach (RectTransform particle in particles)
                if (particle != null) Destroy(particle.gameObject);
        particles = new RectTransform[count];
        particleImages = new Image[count];
        for (int index = 0; index < count; index++)
        {
            GameObject item = new($"Transition Particle {index + 1}", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            item.transform.SetParent(overlay.transform, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(Hash01(index, 0), Hash01(index, 1));
            float size = Mathf.Lerp(18f, 76f, Hash01(index, 2));
            rect.sizeDelta = new Vector2(size, size);
            Image image = item.GetComponent<Image>();
            image.sprite = UiGlowSprite.Get();
            image.raycastTarget = false;
            particles[index] = rect;
            particleImages[index] = image;
        }
    }

    private void UpdateParticles(TransitionProfile profile, bool entering, float progress, bool reduced)
    {
        if (particles == null) return;
        float clampedProgress = Mathf.Clamp01(progress);
        float visibility = entering
            ? Mathf.SmoothStep(0f, 1f, clampedProgress)
            : Mathf.SmoothStep(1f, 0f, clampedProgress);
        for (int index = 0; index < particles.Length; index++)
        {
            Color color = profile.particleColor;
            color.a *= reduced ? .3f : visibility * Mathf.Lerp(.35f, 1f, Hash01(index, 3));
            particleImages[index].color = color;
            float direction = entering ? 1f : -1f;
            particles[index].anchoredPosition = reduced ? Vector2.zero :
                Vector2.up * direction * progress * Mathf.Lerp(20f, 90f, Hash01(index, 4));
        }
    }

    private static float Hash01(int seed, int channel)
    {
        unchecked
        {
            int hash = (seed + 3571) * 374761393 + channel * 668265263;
            hash = (hash ^ (hash >> 13)) * 1274126177;
            return ((hash ^ (hash >> 16)) & 0x7fffffff) / (float)int.MaxValue;
        }
    }
}

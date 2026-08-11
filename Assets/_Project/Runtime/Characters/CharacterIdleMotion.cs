using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class CharacterIdleMotion : MonoBehaviour
{
    private RectTransform rect;
    private Vector2 authoredPosition;
    private Vector3 authoredScale;
    private Quaternion authoredRotation;
    private float elapsed;
    private int seed;
    private CharacterPresentationProfile profile;
    private AccessibilitySettingsService accessibility;

    public void Configure(int deterministicSeed, CharacterPresentationProfile settings)
    {
        rect ??= GetComponent<RectTransform>();
        seed = deterministicSeed;
        profile = settings;
        authoredPosition = rect.anchoredPosition;
        authoredScale = rect.localScale;
        authoredRotation = rect.localRotation;
        elapsed = 0f;
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        AppContext.Services?.TryGet(out accessibility);
    }

    private void Update()
    {
        if (rect == null || profile == null)
            return;
        if (accessibility?.ReducedMotion == true)
        {
            Restore();
            return;
        }
        elapsed += Time.unscaledDeltaTime;
        float breathingCycle = Mathf.Lerp(profile.BreathingCycleRange.x,
            profile.BreathingCycleRange.y, Hash01(seed, 0));
        float breathing = Mathf.Sin(elapsed * Mathf.PI * 2f / breathingCycle
            + Hash01(seed, 1) * Mathf.PI * 2f);
        float swayCycle = Mathf.Lerp(profile.SwayCycleRange.x,
            profile.SwayCycleRange.y, Hash01(seed, 2));
        float sway = Mathf.Sin(elapsed * Mathf.PI * 2f / swayCycle
            + Hash01(seed, 3) * Mathf.PI * 2f);
        float blend = Mathf.SmoothStep(0f, 1f,
            Mathf.Clamp01(elapsed / Mathf.Max(.01f, profile.BlendDuration)));
        rect.anchoredPosition = authoredPosition
            + Vector2.up * breathing * profile.VerticalMotion * blend;
        rect.localScale = new Vector3(
            authoredScale.x,
            authoredScale.y * (1f + breathing * profile.BreathingScale * blend),
            authoredScale.z);
        rect.localRotation = authoredRotation
            * Quaternion.Euler(0f, 0f, sway * profile.SwayDegrees * blend);
    }

    private void OnDisable() => Restore();

    private void Restore()
    {
        if (rect == null)
            return;
        rect.anchoredPosition = authoredPosition;
        rect.localScale = authoredScale;
        rect.localRotation = authoredRotation;
    }

    private static float Hash01(int value, int channel)
    {
        unchecked
        {
            int hash = value * 374761393 + channel * 668265263;
            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash ^= hash >> 16;
            return (hash & 0x7fffffff) / (float)int.MaxValue;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class EvidenceDiscoveryPresenter : MonoBehaviour
{
    [SerializeField] private EvidenceDirector evidence;
    [SerializeField] private TransitionProfile profile;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image image;
    [SerializeField] private Text title;
    [SerializeField] private Text description;
    private AccessibilitySettingsService accessibility;
    private Coroutine routine;

    private void Awake()
    {
        AppContext.Services?.TryGet(out accessibility);
        if (group != null) group.alpha = 0f;
    }

    private void OnEnable()
    {
        if (evidence != null) evidence.EvidenceDiscovered += Present;
    }

    private void OnDisable()
    {
        if (evidence != null) evidence.EvidenceDiscovered -= Present;
        if (routine != null) StopCoroutine(routine);
    }

    private void Present(EvidenceDefinition definition)
    {
        if (definition == null || group == null) return;
        if (image != null) { image.sprite = definition.Image; image.gameObject.SetActive(definition.Image != null); }
        if (title != null) title.text = definition.DisplayName;
        if (description != null) description.text = definition.Description;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        if (accessibility == null) AppContext.Services?.TryGet(out accessibility);
        bool reduced = accessibility?.ReducedMotion == true;
        yield return Fade(0f, 1f, reduced ? 0f : profile?.uiEnterDuration ?? .15f);
        float hold = reduced ? .4f : Mathf.Max(.8f, profile?.holdDuration ?? 0f);
        yield return new WaitForSecondsRealtime(hold);
        yield return Fade(1f, 0f, reduced ? 0f : profile?.uiExitDuration ?? .15f);
        routine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        do
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, duration <= 0f ? 1f : elapsed / duration);
            yield return null;
        } while (elapsed < duration);
        group.alpha = to;
    }
}

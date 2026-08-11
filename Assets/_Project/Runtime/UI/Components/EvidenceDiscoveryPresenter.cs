using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class EvidenceDiscoveryPresenter : MonoBehaviour
{
    [SerializeField] private EvidenceDirector evidence;
    [SerializeField] private EvidenceBoardDirector board;
    [SerializeField] private TransitionProfile profile;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image image;
    [SerializeField] private Text title;
    [SerializeField] private Text description;
    [SerializeField] private Text heading;
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
        if (board != null) board.TheoryReady += PresentTheory;
    }

    private void OnDisable()
    {
        if (evidence != null) evidence.EvidenceDiscovered -= Present;
        if (board != null) board.TheoryReady -= PresentTheory;
        if (routine != null) StopCoroutine(routine);
    }

    private void Present(EvidenceDefinition definition)
    {
        if (definition == null || group == null) return;
        Present(definition.Image, "NEW EVIDENCE", definition.DisplayName, definition.Description);
    }

    private void PresentTheory(TheoryDefinition theory)
    {
        if (theory == null || group == null) return;
        Present(null, "DEDUCTION READY", theory.DisplayName, theory.Description);
    }

    private void Present(Sprite sprite, string headingText, string titleText, string bodyText)
    {
        if (image != null) { image.sprite = sprite; image.gameObject.SetActive(sprite != null); }
        if (heading != null) heading.text = headingText;
        if (title != null) title.text = titleText;
        if (description != null) description.text = bodyText;
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

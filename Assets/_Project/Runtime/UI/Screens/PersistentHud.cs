using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class PersistentHud : MonoBehaviour
{
    [SerializeField]
    private ScreenRouter screens;

    [SerializeField]
    private Button mapButton;

    [SerializeField]
    private Button recordButton;

    [SerializeField]
    private GameStateStore state;
    [SerializeField]
    private GameFlowController flow;
    [SerializeField]
    private ContentDatabase content;

    [SerializeField]
    private Text timeLabel;

    [SerializeField]
    private Text locationLabel;

    [SerializeField]
    private Text objectiveLabel;

    private AccessibilitySettingsService accessibility;
    private Coroutine objectiveTransition;
    private string renderedObjective = string.Empty;

    private const float ObjectiveTransitionDuration = 0.22f;
    private const float ObjectiveTransitionDistance = 18f;

    private void Awake()
    {
        AppContext.Services?.TryGet(out accessibility);
        mapButton?.onClick.AddListener(() => Open(ScreenId.Map));
        recordButton?.onClick.AddListener(() => Open(ScreenId.InvestigationRecord));
        if (screens != null)
            screens.Opened += OnScreenOpened;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (state != null)
            state.Changed += Refresh;
        Refresh(state?.State);
    }

    private void OnDisable()
    {
        if (state != null)
            state.Changed -= Refresh;
    }

    private void OnDestroy()
    {
        if (screens != null)
            screens.Opened -= OnScreenOpened;
    }

    private void OnScreenOpened(ScreenId id)
    {
        bool visible = id == ScreenId.Exploration
            || id == ScreenId.Dialogue
            || id == ScreenId.Investigation
            || id == ScreenId.Interrogation;
        gameObject.SetActive(visible);
    }

    private async void Open(ScreenId id)
    {
        if (screens == null)
            return;
        try
        {
            await screens.OpenAsync(id);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }

    private void Refresh(GameState current)
    {
        current ??= new GameState();
        if (timeLabel != null)
            timeLabel.text = $"DAY {Mathf.Max(1, current.day)} · {TimeBlockLabel(current.timeBlock)}";
        if (locationLabel != null)
            locationLabel.text = ResolveLocation(current.currentLocationId);
        RefreshObjective(current);
    }

    private string ResolveLocation(string id) =>
        content != null && content.TryGetLocation(id, out LocationDefinition location)
            ? location.DisplayName
            : "알 수 없는 위치";

    private void RefreshObjective(GameState current)
    {
        if (objectiveLabel == null)
            return;
        StorySceneDefinition scene = null;
        content?.TryGetStoryScene(current.currentStorySceneId, out scene);
        ObjectiveGuidance guidance = flow != null
            && flow.TryGetPendingTravel(out PendingStorySceneTravel pending)
                ? ObjectiveGuidanceResolver.Resolve(pending)
                : ObjectiveGuidanceResolver.Resolve(scene, state);
        string next = guidance.HudText;
        if (next == renderedObjective)
            return;
        bool animate = !string.IsNullOrEmpty(renderedObjective)
            && objectiveLabel.gameObject.activeInHierarchy
            && accessibility?.ReducedMotion != true;
        renderedObjective = next;
        if (!animate)
        {
            objectiveLabel.text = next;
            return;
        }
        if (objectiveTransition != null)
            StopCoroutine(objectiveTransition);
        objectiveTransition = StartCoroutine(AnimateObjectiveChange(next));
    }

    private IEnumerator AnimateObjectiveChange(string next)
    {
        RectTransform rect = objectiveLabel.rectTransform;
        Vector2 rest = rect.anchoredPosition;
        Color color = objectiveLabel.color;
        for (float elapsed = 0f; elapsed < ObjectiveTransitionDuration; elapsed += Time.unscaledDeltaTime)
        {
            float t = Mathf.Clamp01(elapsed / ObjectiveTransitionDuration);
            float eased = t * t;
            rect.anchoredPosition = rest + Vector2.up * ObjectiveTransitionDistance * eased;
            objectiveLabel.color = new Color(color.r, color.g, color.b, 1f - eased);
            yield return null;
        }
        objectiveLabel.text = next;
        for (float elapsed = 0f; elapsed < ObjectiveTransitionDuration; elapsed += Time.unscaledDeltaTime)
        {
            float t = Mathf.Clamp01(elapsed / ObjectiveTransitionDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            rect.anchoredPosition = rest - Vector2.up * ObjectiveTransitionDistance * (1f - eased);
            objectiveLabel.color = new Color(color.r, color.g, color.b, eased);
            yield return null;
        }
        rect.anchoredPosition = rest;
        objectiveLabel.color = color;
        objectiveTransition = null;
    }

    private static string TimeBlockLabel(TimeBlock block) => block switch
    {
        TimeBlock.Morning => "오전",
        TimeBlock.Afternoon => "오후",
        TimeBlock.Evening => "저녁",
        TimeBlock.Night => "야간",
        _ => "시간 미정",
    };
}

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
    private ContentDatabase content;

    [SerializeField]
    private Text timeLabel;

    [SerializeField]
    private Text locationLabel;

    [SerializeField]
    private Text objectiveLabel;

    private void Awake()
    {
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
        if (objectiveLabel != null)
            objectiveLabel.text = "◆ " + ResolveObjective(current.currentStorySceneId);
    }

    private string ResolveLocation(string id) =>
        content != null && content.TryGetLocation(id, out LocationDefinition location)
            ? location.DisplayName
            : id ?? string.Empty;

    private string ResolveObjective(string id) =>
        content != null && content.TryGetStoryScene(id, out StorySceneDefinition scene)
            ? scene.DisplayName
            : "자유 조사";

    private static string TimeBlockLabel(TimeBlock block) => block switch
    {
        TimeBlock.Morning => "오전",
        TimeBlock.Afternoon => "오후",
        TimeBlock.Evening => "저녁",
        TimeBlock.Night => "야간",
        _ => "시간 미정",
    };
}

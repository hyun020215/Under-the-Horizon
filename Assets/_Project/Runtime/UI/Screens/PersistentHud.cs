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
    private Text timeLabel;

    [SerializeField]
    private Text anxietyLabel;

    [SerializeField]
    private Text integrityLabel;

    [SerializeField]
    private Image anxietyFill;

    [SerializeField]
    private Image integrityFill;

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
        bool visible = id != ScreenId.Title
            && id != ScreenId.SaveSlot
            && id != ScreenId.Ending
            && id != ScreenId.Credits;
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
            timeLabel.text = $"{Mathf.Max(1, current.day)}일 차  ·  {TimeBlockLabel(current.timeBlock)}";
        if (anxietyLabel != null)
            anxietyLabel.text = $"승객 불안  {current.publicAnxiety}/100";
        if (integrityLabel != null)
            integrityLabel.text = $"현장 보존도  {current.evidenceIntegrity}/100";
        if (anxietyFill != null)
            anxietyFill.fillAmount = current.publicAnxiety / 100f;
        if (integrityFill != null)
            integrityFill.fillAmount = current.evidenceIntegrity / 100f;
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

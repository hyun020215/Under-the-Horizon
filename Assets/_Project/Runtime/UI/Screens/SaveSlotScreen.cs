using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class SaveSlotScreen : ScreenBase
{
    [SerializeField]
    private Button[] slotButtons;
    [SerializeField]
    private Text[] chapterLabels;
    [SerializeField]
    private Text[] statusLabels;
    [SerializeField]
    private Text[] actionLabels;

    private TaskCompletionSource<SaveSlot> selection;

    private void Awake()
    {
        if (slotButtons == null)
            return;

        for (var index = 0; index < slotButtons.Length; index++)
        {
            int slotIndex = index;
            slotButtons[index]?.onClick.AddListener(() => Select(slotIndex));
        }
    }

    public override Task OpenAsync(ScreenContext context)
    {
        RefreshCards();
        return base.OpenAsync(context);
    }

    public Task<SaveSlot> WaitForSelectionAsync()
    {
        selection ??= new TaskCompletionSource<SaveSlot>();
        return selection.Task;
    }

    private void Select(int index)
    {
        selection ??= new TaskCompletionSource<SaveSlot>();
        selection.TrySetResult(new SaveSlot(index));
    }

    private void RefreshCards()
    {
        SaveService saves = null;
        AppContext.Services?.TryGet(out saves);
        saves ??= new SaveService();
        for (var index = 0; index < slotButtons.Length; index++)
        {
            SaveSlot slot = new(index);
            bool occupied = saves.Exists(slot);
            GameState state = occupied ? saves.Load(slot) : null;
            SetText(chapterLabels, index,
                occupied ? $"DAY {Mathf.Max(1, state.day)}" : "빈 항해 기록");
            string location = occupied && !string.IsNullOrWhiteSpace(state.currentLocationId)
                ? state.currentLocationId
                : occupied ? state.currentStorySceneId : "새로운 수사를 시작합니다";
            SetText(statusLabels, index, location);
            SetText(actionLabels, index, occupied ? "이어하기" : "새로 시작");
        }
    }

    private static void SetText(Text[] labels, int index, string value)
    {
        if (labels != null && index < labels.Length && labels[index] != null)
            labels[index].text = value;
    }
}

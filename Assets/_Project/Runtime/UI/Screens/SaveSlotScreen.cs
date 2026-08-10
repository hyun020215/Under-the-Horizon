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
    [SerializeField]
    private Button[] deleteButtons;
    [SerializeField]
    private ModalRouter modals;

    private TaskCompletionSource<SaveSlot> selection;

    private void Awake()
    {
        if (slotButtons == null)
            return;

        for (var index = 0; index < slotButtons.Length; index++)
        {
            int slotIndex = index;
            slotButtons[index]?.onClick.AddListener(() => Select(slotIndex));
            deleteButtons?[index]?.onClick.AddListener(() => Delete(slotIndex));
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

    private async void Select(int index)
    {
        SaveService saves = ResolveSaves();
        bool occupied = saves.Exists(new SaveSlot(index));
        string action = occupied ? "이어하기" : "새로 시작";
        if (modals != null && !await modals.ConfirmAsync(
                $"항해 기록 {index + 1}에서 {action}하시겠습니까?"))
            return;
        selection ??= new TaskCompletionSource<SaveSlot>();
        selection.TrySetResult(new SaveSlot(index));
    }

    private async void Delete(int index)
    {
        SaveSlot slot = new(index);
        SaveService saves = ResolveSaves();
        if (!saves.Exists(slot))
            return;
        if (modals != null && !await modals.ConfirmAsync(
                $"항해 기록 {index + 1}을 삭제하시겠습니까?\n삭제한 기록은 복구할 수 없습니다."))
            return;
        saves.Delete(slot);
        RefreshCards();
    }

    private void RefreshCards()
    {
        SaveService saves = ResolveSaves();
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
            if (deleteButtons != null && index < deleteButtons.Length &&
                deleteButtons[index] != null)
                deleteButtons[index].gameObject.SetActive(occupied);
        }
    }

    private static SaveService ResolveSaves()
    {
        SaveService saves = null;
        AppContext.Services?.TryGet(out saves);
        return saves ?? new SaveService();
    }

    private static void SetText(Text[] labels, int index, string value)
    {
        if (labels != null && index < labels.Length && labels[index] != null)
            labels[index].text = value;
    }
}

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class SaveSlotScreen : ScreenBase
{
    [SerializeField]
    private Button[] slotButtons;

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

    public Task<SaveSlot> WaitForSelectionAsync()
    {
        selection ??= new TaskCompletionSource<SaveSlot>();
        return selection.Task;
    }

    private void Select(int index)
    {
        selection?.TrySetResult(new SaveSlot(index));
        selection = null;
    }
}

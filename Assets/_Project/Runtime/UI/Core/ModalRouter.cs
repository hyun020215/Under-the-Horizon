using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class ModalRouter : MonoBehaviour
{
    [SerializeField]
    private GameObject confirm;

    [SerializeField]
    private GameObject pause;
    [SerializeField]
    private ConfirmDialog confirmDialog;
    private readonly Stack<GameObject> stack = new();

    public Task OpenAsync(ModalId id)
    {
        GameObject view =
            id == ModalId.Confirm ? confirm
            : id == ModalId.Pause ? pause
            : null;
        if (view != null)
        {
            view.SetActive(true);
            stack.Push(view);
        }
        return Task.CompletedTask;
    }

    public void CloseTop()
    {
        if (stack.Count > 0)
            stack.Pop().SetActive(false);
    }

    public async Task<bool> ConfirmAsync(string message)
    {
        if (confirmDialog == null)
            return false;
        GameObject view = confirmDialog.gameObject;
        stack.Push(view);
        bool confirmed = await confirmDialog.PresentAsync(message);
        if (stack.Count > 0 && stack.Peek() == view)
            CloseTop();
        else
            view.SetActive(false);
        return confirmed;
    }
}

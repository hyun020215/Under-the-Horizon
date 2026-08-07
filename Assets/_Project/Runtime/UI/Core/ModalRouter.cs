using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class ModalRouter : MonoBehaviour
{
    [SerializeField]
    private GameObject confirm;

    [SerializeField]
    private GameObject pause;
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
}

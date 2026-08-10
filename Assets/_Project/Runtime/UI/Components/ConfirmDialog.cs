using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class ConfirmDialog : MonoBehaviour
{
    [SerializeField] private Text messageLabel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    private TaskCompletionSource<bool> pending;

    private void Awake()
    {
        confirmButton?.onClick.AddListener(() => Complete(true));
        cancelButton?.onClick.AddListener(() => Complete(false));
    }

    public Task<bool> PresentAsync(string message)
    {
        pending?.TrySetResult(false);
        pending = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        if (messageLabel != null)
            messageLabel.text = message ?? string.Empty;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        return pending.Task;
    }

    private void Complete(bool confirmed)
    {
        TaskCompletionSource<bool> completion = pending;
        pending = null;
        completion?.TrySetResult(confirmed);
    }

    private void OnDisable()
    {
        pending?.TrySetResult(false);
        pending = null;
    }
}

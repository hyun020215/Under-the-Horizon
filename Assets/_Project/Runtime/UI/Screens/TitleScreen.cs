using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class TitleScreen : ScreenBase
{
    [SerializeField] private Button startButton;

    private TaskCompletionSource<bool> startRequested;

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(RequestStart);
    }

    public Task WaitForStartAsync()
    {
        startRequested ??= new TaskCompletionSource<bool>();
        return startRequested.Task;
    }

    private void RequestStart()
    {
        startRequested?.TrySetResult(true);
        startRequested = null;
    }
}

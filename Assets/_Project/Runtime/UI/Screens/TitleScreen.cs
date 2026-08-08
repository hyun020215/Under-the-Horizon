using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class TitleScreen : ScreenBase
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private ScreenRouter screens;

    private TaskCompletionSource<bool> startRequested;

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(RequestStart);
        settingsButton?.onClick.AddListener(() => Open(ScreenId.Settings));
        creditsButton?.onClick.AddListener(() => Open(ScreenId.Credits));
        quitButton?.onClick.AddListener(Quit);
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

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

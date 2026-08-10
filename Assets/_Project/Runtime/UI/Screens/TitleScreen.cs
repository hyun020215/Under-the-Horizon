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
    [SerializeField] private AmbientParticleProfile ambientParticles;

    private TaskCompletionSource<bool> startRequested;

    private void Awake()
    {
        EnsureAtmosphere();
        if (startButton != null)
            startButton.onClick.AddListener(RequestStart);
        settingsButton?.onClick.AddListener(() => Open(ScreenId.Settings));
        creditsButton?.onClick.AddListener(() => Open(ScreenId.Credits));
        quitButton?.onClick.AddListener(Quit);
    }

    private void EnsureAtmosphere()
    {
        Transform background = transform.Find("Title Background");
        if (background == null)
            return;
        AmbientParticleOverlay overlay =
            background.GetComponent<AmbientParticleOverlay>()
            ?? background.gameObject.AddComponent<AmbientParticleOverlay>();
        overlay.Initialize((RectTransform)background, ambientParticles);
    }

    public Task WaitForStartAsync()
    {
        startRequested ??= new TaskCompletionSource<bool>();
        return startRequested.Task;
    }

    private void RequestStart()
    {
        startRequested ??= new TaskCompletionSource<bool>();
        startRequested.TrySetResult(true);
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

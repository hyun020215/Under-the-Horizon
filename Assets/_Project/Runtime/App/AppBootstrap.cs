using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class AppBootstrap : MonoBehaviour
{
    internal static System.Func<SaveService> SaveServiceFactoryOverride { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => SaveServiceFactoryOverride = null;

    [SerializeField]
    private string gameScene = "Game";

    [SerializeField]
    private GameDefinition gameDefinition;

    private ContentLoader contentLoader;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        AppContext.Services = new AppServiceRegistry();

        try
        {
            contentLoader = new ContentLoader(gameDefinition);
            SaveService saves = CreateSaveService();
            var audioSettings = new AudioSettingsService();
            var displaySettings = new DisplaySettingsService();
            var accessibilitySettings = new AccessibilitySettingsService();
            audioSettings.Load();
            displaySettings.Load();
            accessibilitySettings.Load();

            AppContext.Services.Register(gameDefinition);
            AppContext.Services.Register(contentLoader.Database);
            AppContext.Services.Register(contentLoader);
            AppContext.Services.Register(saves);
            AppContext.Services.Register(audioSettings);
            AppContext.Services.Register(displaySettings);
            AppContext.Services.Register(accessibilitySettings);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, this);
            enabled = false;
        }
    }

    private static SaveService CreateSaveService() =>
        SaveServiceFactoryOverride?.Invoke() ?? new SaveService();

    private IEnumerator Start()
    {
        if (contentLoader == null)
            yield break;

        if (SceneManager.GetActiveScene().name != gameScene)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(gameScene);
            while (operation != null && !operation.isDone)
                yield return null;
        }

        RegisterSceneServices();
    }

    private static void RegisterSceneServices()
    {
        GameStateStore state = FindFirstObjectByType<GameStateStore>();
        AudioDirector audio = FindFirstObjectByType<AudioDirector>();
        InteractionFeedbackService interactionFeedback =
            FindFirstObjectByType<InteractionFeedbackService>();

        if (state != null)
            AppContext.Services.Register(state);
        if (audio != null)
        {
            AppContext.Services.Register(audio);
            if (AppContext.Services.TryGet(out AudioSettingsService settings))
                settings.Apply(audio);
        }
        if (interactionFeedback != null)
            AppContext.Services.Register(interactionFeedback);
    }
}

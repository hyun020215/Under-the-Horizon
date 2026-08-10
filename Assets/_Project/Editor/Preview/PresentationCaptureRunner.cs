using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PresentationCaptureRunner
{
    private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
    private static int frame;
    private static int stage;
    private static string outputDirectory;

    public static void CaptureFromCommandLine()
    {
        frame = 0;
        stage = 0;
        outputDirectory = Path.GetFullPath("Logs/PresentationCaptures");
        Directory.CreateDirectory(outputDirectory);
        foreach (string fileName in new[]
                 { "01-title.png", "02-save-slots.png", "03-gameplay.png" })
        {
            string path = Path.Combine(outputDirectory, fileName);
            if (File.Exists(path))
                File.Delete(path);
        }
        EditorSceneManager.OpenScene(GameScenePath);
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredPlayMode)
        {
            Screen.SetResolution(1600, 900, false);
            EditorApplication.update += Update;
        }
        else if (change == PlayModeStateChange.EnteredEditMode && stage >= 6)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.Exit(0);
        }
    }

    private static void Update()
    {
        frame++;
        if (stage == 0 && frame >= 90)
        {
            Capture("01-title.png");
            NextStage();
        }
        else if (stage == 1 && frame >= 20)
        {
            Click("StartButton");
            NextStage();
        }
        else if (stage == 2 && frame >= 90)
        {
            Capture("02-save-slots.png");
            NextStage();
        }
        else if (stage == 3 && frame >= 20)
        {
            Click("Slot1Button");
            NextStage();
        }
        else if (stage == 4 && frame >= 20)
        {
            Click("ConfirmButton");
            NextStage();
        }
        else if (stage == 5 && frame >= 240)
        {
            Capture("03-gameplay.png");
            stage = 6;
            EditorApplication.update -= Update;
            EditorApplication.isPlaying = false;
        }
    }

    private static void Capture(string fileName) =>
        ScreenCapture.CaptureScreenshot(Path.Combine(outputDirectory, fileName), 1);

    private static void Click(string name)
    {
        foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (button.name != name)
                continue;
            button.onClick.Invoke();
            return;
        }
        Debug.LogError($"Presentation capture could not find button '{name}'.");
    }

    private static void NextStage()
    {
        stage++;
        frame = 0;
    }
}

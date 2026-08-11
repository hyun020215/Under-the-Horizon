using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ResponsiveUiCaptureRunner
{
    private static readonly (int width, int height)[] Resolutions =
    {
        (1280, 720), (1920, 1080), (2560, 1440),
    };

    private static readonly string[] Prefabs =
    {
        "PF_TitleScreen", "PF_SaveSlotScreen", "PF_ExplorationScreen",
        "PF_DialogueScreen", "PF_MapScreen", "PF_InvestigationScreen",
        "PF_RecordScreen", "PF_InterrogationScreen", "PF_EvidenceBoardScreen",
        "PF_ReconstructionScreen", "PF_PuzzleScreen", "PF_EndingScreen",
        "PF_CreditsScreen", "PF_SettingsScreen",
    };

    public static void CaptureFromCommandLine()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string output = Path.Combine(projectRoot, "Logs", "ResponsiveUiCaptures");
        Directory.CreateDirectory(output);
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = CreateCamera();
        Canvas canvas = CreateCanvas(camera);
        foreach ((int width, int height) in Resolutions)
        foreach (string prefabName in Prefabs)
            CapturePrefab(canvas, camera, prefabName, width, height, output);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Debug.Log($"Responsive UI captures complete: {Resolutions.Length * Prefabs.Length} images at {output}.");
        EditorApplication.Exit(0);
    }

    private static Camera CreateCamera()
    {
        GameObject root = new("Responsive Capture Camera", typeof(Camera));
        Camera camera = root.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.004f, .008f, .014f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        return camera;
    }

    private static Canvas CreateCanvas(Camera camera)
    {
        GameObject root = new("Responsive Capture Canvas", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .5f;
        return canvas;
    }

    private static void CapturePrefab(
        Canvas canvas, Camera camera, string prefabName, int width, int height, string output)
    {
        string path = $"Assets/_Project/Prefabs/UI/{prefabName}.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            throw new InvalidOperationException($"Missing responsive capture prefab: {path}");
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
        instance.name = prefabName;
        instance.SetActive(true);
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        Canvas.ForceUpdateCanvases();
        RenderTexture target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        Texture2D image = new(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply(false);
        File.WriteAllBytes(Path.Combine(output, $"{width}x{height}_{prefabName}.png"), image.EncodeToPNG());
        RenderTexture.active = previous;
        camera.targetTexture = null;
        RenderTexture.ReleaseTemporary(target);
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(instance);
    }
}

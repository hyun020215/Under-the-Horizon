using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class PlayerBuildSmokeRunner
{
    private const string BootstrapScenePath = "Assets/_Project/Scenes/Bootstrap.unity";
    private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
    private const string OutputRelativePath = "Build/Smoke/Windows64/UnderTheHorizon.exe";
    private static readonly string[] RequiredScenePaths =
    {
        BootstrapScenePath,
        GameScenePath,
    };

    [MenuItem("Under The Horizon/Build/Windows 64-bit Player Smoke")]
    public static void Run()
    {
        BuildPreflightValidator.Run();

        string[] scenePaths = EditorBuildSettings.scenes
            .Where(scene => scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
            .Select(scene => scene.path)
            .ToArray();

        if (!scenePaths.SequenceEqual(RequiredScenePaths, StringComparer.Ordinal))
        {
            throw new BuildFailedException(
                "Enabled build scenes must be exactly Bootstrap then Game. " +
                $"Current order: [{string.Join(", ", scenePaths)}].");
        }

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
            throw new BuildFailedException($"Could not resolve the project root from {Application.dataPath}.");

        string outputPath = Path.Combine(projectRoot, OutputRelativePath);
        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(outputDirectory))
            throw new BuildFailedException($"Invalid smoke build output path: {outputPath}");
        Directory.CreateDirectory(outputDirectory);

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development,
        });

        if (report == null)
            throw new BuildFailedException("Windows 64-bit smoke build did not return a build report.");

        BuildSummary summary = report.summary;
        if (summary.result != BuildResult.Succeeded)
        {
            throw new BuildFailedException(
                $"Windows 64-bit smoke build failed with result {summary.result}. " +
                $"Errors: {summary.totalErrors}, warnings: {summary.totalWarnings}.");
        }

        Debug.Log(
            $"Under the Horizon Windows 64-bit smoke build passed: {summary.outputPath} " +
            $"({summary.totalSize} bytes, {summary.totalTime}).");
    }
}

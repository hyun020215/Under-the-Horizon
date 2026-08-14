using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class RenderingSettingsValidator
{
    public const string CanonicalRendererPath =
        "Assets/_Project/Settings/Rendering/UTH_Renderer.asset";
    public const string CanonicalPipelinePath =
        "Assets/_Project/Settings/Rendering/UTH_RenderPipeline.asset";
    public const string CanonicalGlobalSettingsPath =
        "Assets/_Project/Settings/Rendering/UTH_URPGlobalSettings.asset";
    public const string CanonicalVolumeProfilePath =
        "Assets/_Project/Settings/Rendering/UTH_GlobalVolume.asset";

    private const string RootGlobalSettingsFallbackPath =
        "Assets/UniversalRenderPipelineGlobalSettings.asset";
    private const string RootVolumeProfileFallbackPath =
        "Assets/DefaultVolumeProfile.asset";

    public static List<string> ValidateAll()
    {
        var errors = new List<string>();

        Renderer2DData canonicalRenderer =
            LoadCanonicalAsset<Renderer2DData>(CanonicalRendererPath, errors);
        UniversalRenderPipelineAsset canonicalPipeline =
            LoadCanonicalAsset<UniversalRenderPipelineAsset>(CanonicalPipelinePath, errors);
        RenderPipelineGlobalSettings canonicalGlobalSettings =
            LoadCanonicalAsset<RenderPipelineGlobalSettings>(CanonicalGlobalSettingsPath, errors);
        VolumeProfile canonicalVolumeProfile =
            LoadCanonicalAsset<VolumeProfile>(CanonicalVolumeProfilePath, errors);

        ValidateRootFallbackIsAbsent(RootGlobalSettingsFallbackPath, errors);
        ValidateRootFallbackIsAbsent(RootVolumeProfileFallbackPath, errors);

        ValidateGraphicsPipeline(canonicalPipeline, errors);
        ValidateQualityPipelines(canonicalPipeline, errors);
        ValidatePipeline(canonicalPipeline, canonicalRenderer, errors);
        ValidateGlobalSettings(canonicalGlobalSettings, canonicalVolumeProfile, errors);
        ValidateVolumeProfile(canonicalVolumeProfile, errors);

        return errors;
    }

    private static T LoadCanonicalAsset<T>(string assetPath, ICollection<string> errors)
        where T : UnityEngine.Object
    {
        string absolutePath = ToAbsolutePath(assetPath);
        if (!File.Exists(absolutePath))
        {
            errors.Add($"Required rendering asset is missing: {assetPath}.");
            return null;
        }

        if (new FileInfo(absolutePath).Length == 0)
        {
            errors.Add($"Required rendering asset is empty: {assetPath}.");
            return null;
        }

        UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (asset == null)
        {
            errors.Add($"Required rendering asset is not loadable: {assetPath}.");
            return null;
        }

        if (asset is T typedAsset)
            return typedAsset;

        errors.Add(
            $"Rendering asset {assetPath} must be {typeof(T).Name}, " +
            $"but was {asset.GetType().Name}.");
        return null;
    }

    private static void ValidateGraphicsPipeline(
        UniversalRenderPipelineAsset canonicalPipeline,
        ICollection<string> errors)
    {
        if (canonicalPipeline == null)
            return;

        if (GraphicsSettings.defaultRenderPipeline == canonicalPipeline)
            return;

        errors.Add(
            "GraphicsSettings.defaultRenderPipeline must reference " +
            $"{CanonicalPipelinePath}, but referenced " +
            $"{DescribeAsset(GraphicsSettings.defaultRenderPipeline)}.");
    }

    private static void ValidateQualityPipelines(
        UniversalRenderPipelineAsset canonicalPipeline,
        ICollection<string> errors)
    {
        if (canonicalPipeline == null)
            return;

        string[] qualityNames = QualitySettings.names;
        for (int index = 0; index < qualityNames.Length; index++)
        {
            RenderPipelineAsset qualityPipeline =
                QualitySettings.GetRenderPipelineAssetAt(index);
            if (qualityPipeline == canonicalPipeline)
                continue;

            errors.Add(
                $"Quality level '{qualityNames[index]}' must reference " +
                $"{CanonicalPipelinePath}, but referenced {DescribeAsset(qualityPipeline)}.");
        }
    }

    private static void ValidatePipeline(
        UniversalRenderPipelineAsset canonicalPipeline,
        Renderer2DData canonicalRenderer,
        ICollection<string> errors)
    {
        if (canonicalPipeline == null)
            return;

        if (canonicalPipeline.rendererDataList.Length == 0)
        {
            errors.Add(
                $"{CanonicalPipelinePath} must define a default 2D renderer.");
        }
        else
        {
            for (int index = 0; index < canonicalPipeline.rendererDataList.Length; index++)
            {
                if (canonicalPipeline.rendererDataList[index] == null)
                {
                    errors.Add(
                        $"{CanonicalPipelinePath} contains a missing renderer " +
                        $"reference at index {index}.");
                }
            }

            var serializedPipeline = new SerializedObject(canonicalPipeline);
            SerializedProperty defaultRendererIndexProperty =
                serializedPipeline.FindProperty("m_DefaultRendererIndex");
            if (defaultRendererIndexProperty == null)
            {
                errors.Add(
                    $"{CanonicalPipelinePath} does not expose a default renderer index.");
            }
            else
            {
                int defaultRendererIndex = defaultRendererIndexProperty.intValue;
                if (defaultRendererIndex < 0
                    || defaultRendererIndex >= canonicalPipeline.rendererDataList.Length)
                {
                    errors.Add(
                        $"{CanonicalPipelinePath} default renderer index " +
                        $"{defaultRendererIndex} is out of range.");
                }
                else if (canonicalRenderer != null
                    && canonicalPipeline.rendererDataList[defaultRendererIndex]
                        != canonicalRenderer)
                {
                    errors.Add(
                        $"{CanonicalPipelinePath} default renderer must reference " +
                        $"{CanonicalRendererPath}, but referenced " +
                        $"{DescribeAsset(canonicalPipeline.rendererDataList[defaultRendererIndex])}.");
                }
            }
        }

    }

    private static void ValidateVolumeProfile(
        VolumeProfile canonicalVolumeProfile,
        ICollection<string> errors)
    {
        if (canonicalVolumeProfile == null)
            return;

        if (canonicalVolumeProfile.components.Count == 0)
        {
            errors.Add(
                $"{CanonicalVolumeProfilePath} must contain Unity's default Volume overrides. " +
                "Run the canonical Player build authoring path before committing it.");
        }

        for (int index = 0; index < canonicalVolumeProfile.components.Count; index++)
        {
            if (canonicalVolumeProfile.components[index] == null)
            {
                errors.Add(
                    $"{CanonicalVolumeProfilePath} contains a missing Volume override " +
                    $"at index {index}.");
            }
        }
    }

    private static void ValidateGlobalSettings(
        RenderPipelineGlobalSettings canonicalGlobalSettings,
        VolumeProfile canonicalVolumeProfile,
        ICollection<string> errors)
    {
        RenderPipelineGlobalSettings registeredGlobalSettings =
            GraphicsSettings.GetSettingsForRenderPipeline<UniversalRenderPipeline>();

        if (canonicalGlobalSettings != null
            && registeredGlobalSettings != canonicalGlobalSettings)
        {
            errors.Add(
                "URP global settings must reference " +
                $"{CanonicalGlobalSettingsPath}, but referenced " +
                $"{DescribeAsset(registeredGlobalSettings)}.");
        }

        URPDefaultVolumeProfileSettings defaultVolumeSettings =
            GraphicsSettings.GetRenderPipelineSettings<URPDefaultVolumeProfileSettings>();
        if (defaultVolumeSettings == null)
        {
            errors.Add("URP global settings do not define default volume profile settings.");
            return;
        }

        if (canonicalVolumeProfile != null
            && defaultVolumeSettings.volumeProfile != canonicalVolumeProfile)
        {
            errors.Add(
                "URP default volume profile must reference " +
                $"{CanonicalVolumeProfilePath}, but referenced " +
                $"{DescribeAsset(defaultVolumeSettings.volumeProfile)}.");
        }
    }

    private static void ValidateRootFallbackIsAbsent(
        string assetPath,
        ICollection<string> errors)
    {
        string absolutePath = ToAbsolutePath(assetPath);
        bool assetExists = File.Exists(absolutePath);
        bool metaExists = File.Exists(absolutePath + ".meta");
        if (!assetExists && !metaExists)
            return;

        errors.Add(
            $"Root-level URP fallback must not exist: {assetPath}. " +
            "Use the canonical asset under Assets/_Project/Settings/Rendering instead.");
    }

    private static string DescribeAsset(UnityEngine.Object asset)
    {
        if (asset == null)
            return "null";

        string path = AssetDatabase.GetAssetPath(asset);
        return string.IsNullOrEmpty(path) ? asset.name : path;
    }

    private static string ToAbsolutePath(string assetPath)
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        if (string.IsNullOrEmpty(projectRoot))
            return assetPath;

        return Path.GetFullPath(Path.Combine(
            projectRoot,
            assetPath.Replace('/', Path.DirectorySeparatorChar)));
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ArtAssetMaintenance
{
    private const string ArtRoot = "Assets/_Project/Art";

    private static readonly (string Source, string Destination)[] FolderMoves =
    {
        ($"{ArtRoot}/Backgrounds/Locations/Variants", $"{ArtRoot}/Backgrounds/Variants"),
        ($"{ArtRoot}/Characters/Runtime", $"{ArtRoot}/Characters/ConceptSheets"),
        ($"{ArtRoot}/UI/Overhaul", $"{ArtRoot}/UI/Screens"),
    };

    private static readonly (string Source, string Destination)[] AssetMoves =
    {
        (
            $"{ArtRoot}/UI/Maps/UI_map_mv_elysium_cutaway.png",
            $"{ArtRoot}/UI/Map/UI_map_mv_elysium_cutaway.png"
        ),
        ($"{ArtRoot}/Maps/UI/MAP_ui_map_screen_backdrop.png", $"{ArtRoot}/UI/Map/MAP_ui_map_screen_backdrop.png"),
        ($"{ArtRoot}/UI/Screens/UI_logo_transparent.png", $"{ArtRoot}/Branding/UI_logo_transparent.png"),
    };

    private static readonly Dictionary<string, string> CharacterFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["adrian_vale"] = "Adrian",
        ["claire_hawthorne"] = "Claire",
        ["daniel_mercer"] = "Daniel",
        ["evelyn_shaw"] = "Evelyn",
        ["helena_ward"] = "Helena",
        ["marcus_bell"] = "Marcus",
        ["owen_price"] = "Owen",
        ["richard_hawthorne"] = "Richard",
        ["thomas_reed"] = "Thomas",
    };

    private static readonly Dictionary<string, string> LegacyCharacterFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["01_elysium_manager"] = "Evelyn",
        ["02_naval_captain"] = "Thomas",
        ["03_chief_engineer"] = "Marcus",
        ["04_detective"] = "Adrian",
        ["blonde_woman"] = "Claire",
        ["doctor"] = "Owen",
        ["elderly_man"] = "Richard",
        ["journalist"] = "Daniel",
        ["security_guard"] = "Helena",
    };

    private static readonly HashSet<string> ProfessionalNpcNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "archivist",
        "atrium_guide",
        "ballroom_musician",
        "dining_sommelier",
        "dock_porter",
        "robotics_tech",
        "suite_steward",
        "VIP_host",
    };

    [MenuItem("Under the Horizon/Art/정리 및 고화질 임포트 적용")]
    public static void OrganizeAndApplyQualitySettings()
    {
        foreach ((string source, string destination) in FolderMoves)
        {
            MoveFolder(source, destination);
        }

        foreach ((string source, string destination) in AssetMoves)
        {
            MoveAsset(source, destination);
        }

        OrganizeCharacters();
        OrganizeInvestigationArt();
        MergeFolder($"{ArtRoot}/UI/Runtime/Icons", $"{ArtRoot}/UI/Icons");

        DeleteEmptyMigrationFolders();
        ApplyHighQualityImportSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Art 자산 역할별 정리와 고화질 임포트 설정을 완료했습니다.");
    }

    public static void RunFromCommandLine()
    {
        OrganizeAndApplyQualitySettings();
    }

    private static void MoveFolder(string source, string destination)
    {
        if (!AssetDatabase.IsValidFolder(source))
        {
            return;
        }

        if (AssetDatabase.IsValidFolder(destination))
        {
            MergeFolder(source, destination);
            return;
        }

        EnsureFolder(Path.GetDirectoryName(destination)?.Replace('\\', '/'));
        string error = AssetDatabase.MoveAsset(source, destination);
        if (!string.IsNullOrEmpty(error))
        {
            throw new InvalidOperationException($"Art 폴더 이동 실패: {source} -> {destination}: {error}");
        }
    }

    private static void MoveAsset(string source, string destination)
    {
        if (AssetDatabase.LoadMainAssetAtPath(source) == null)
        {
            return;
        }

        EnsureFolder(Path.GetDirectoryName(destination)?.Replace('\\', '/'));
        string error = AssetDatabase.MoveAsset(source, destination);
        if (!string.IsNullOrEmpty(error))
        {
            throw new InvalidOperationException($"Art 자산 이동 실패: {source} -> {destination}: {error}");
        }
    }

    private static void MergeFolder(string source, string destination)
    {
        if (!AssetDatabase.IsValidFolder(source))
        {
            return;
        }

        EnsureFolder(destination);
        string sourceSystemPath = Path.GetFullPath(source);
        foreach (string file in Directory.GetFiles(sourceSystemPath, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string relative = file.Substring(sourceSystemPath.Length + 1).Replace('\\', '/');
            string sourceAsset = $"{source}/{relative}";
            string destinationAsset = $"{destination}/{relative}";
            MoveAsset(sourceAsset, destinationAsset);
        }

        AssetDatabase.DeleteAsset(source);
    }

    private static void OrganizeCharacters()
    {
        foreach (KeyValuePair<string, string> entry in CharacterFolders)
        {
            string token = entry.Key;
            string character = entry.Value;
            MoveAsset(
                $"{ArtRoot}/Characters/World/CHR_{token}.png",
                $"{ArtRoot}/Characters/{character}/FullBody/CHR_{token}.png"
            );
            MoveAsset(
                $"{ArtRoot}/Characters/Expressions/CHR_portrait_{token}_expressions.png",
                $"{ArtRoot}/Characters/{character}/Expressions/CHR_portrait_{token}_expressions.png"
            );
            MoveAsset(
                $"{ArtRoot}/Characters/ConceptSheets/CHR_{token}.png",
                $"{ArtRoot}/Characters/{character}/Concept/CHR_{token}_concept.png"
            );
        }

        MoveAsset(
            $"{ArtRoot}/Characters/ConceptSheets/CHR_marcus_bell_and_helena_ward.png",
            $"{ArtRoot}/Characters/Marcus/Concept/CHR_marcus_bell_and_helena_ward_concept.png"
        );

        foreach (KeyValuePair<string, string> entry in LegacyCharacterFolders)
        {
            string token = entry.Key;
            string character = entry.Value;
            MoveFirstMatchingAsset(
                $"{ArtRoot}/Characters/Busts",
                $"CHR_{token}",
                $"{ArtRoot}/Characters/{character}/Portrait"
            );
            MoveFirstMatchingAsset(
                $"{ArtRoot}/Characters/FullBody",
                $"CHR_{token}",
                $"{ArtRoot}/Characters/{character}/FullBody"
            );
        }

        OrganizeAmbientCharacters();
    }

    private static void OrganizeAmbientCharacters()
    {
        string source = $"{ArtRoot}/Characters/Ambient";
        if (!AssetDatabase.IsValidFolder(source))
        {
            return;
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { source }))
        {
            string sourceAsset = AssetDatabase.GUIDToAssetPath(guid);
            string filename = Path.GetFileNameWithoutExtension(sourceAsset);
            string token = filename.StartsWith("CHR_", StringComparison.OrdinalIgnoreCase)
                ? filename.Substring(4)
                : filename;
            token = token.Replace("_expressions", string.Empty);
            string category = ProfessionalNpcNames.Contains(token) ? "ProfessionalNPC" : "AmbientNPC";
            MoveAsset(sourceAsset, $"{ArtRoot}/Characters/{category}/{Path.GetFileName(sourceAsset)}");
        }
    }

    private static void OrganizeInvestigationArt()
    {
        MergeFolder(
            $"{ArtRoot}/Evidence/Investigation/BodyDiscovery",
            $"{ArtRoot}/Investigation/Horizon/BodyDiscovery"
        );
        MergeFolder(
            $"{ArtRoot}/Props/Puzzles",
            $"{ArtRoot}/Investigation/Horizon/Puzzles"
        );
        MergeFolder(
            $"{ArtRoot}/Investigation/BodyDiscovery",
            $"{ArtRoot}/Investigation/Horizon/BodyDiscovery"
        );
        MergeFolder(
            $"{ArtRoot}/Investigation/Puzzles",
            $"{ArtRoot}/Investigation/Horizon/Puzzles"
        );
    }

    private static void MoveFirstMatchingAsset(string sourceFolder, string prefix, string destinationFolder)
    {
        if (!AssetDatabase.IsValidFolder(sourceFolder))
        {
            return;
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { sourceFolder }))
        {
            string source = AssetDatabase.GUIDToAssetPath(guid);
            if (!Path.GetFileNameWithoutExtension(source).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            MoveAsset(source, $"{destinationFolder}/{Path.GetFileName(source)}");
            return;
        }
    }

    private static void EnsureFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }

    private static void DeleteEmptyMigrationFolders()
    {
        string[] candidates =
        {
            $"{ArtRoot}/UI/Runtime",
            $"{ArtRoot}/UI/Maps",
            $"{ArtRoot}/Characters/World",
            $"{ArtRoot}/Characters/Expressions",
            $"{ArtRoot}/Characters/ConceptSheets",
            $"{ArtRoot}/Characters/Busts",
            $"{ArtRoot}/Characters/FullBody",
            $"{ArtRoot}/Characters/Ambient",
            $"{ArtRoot}/Evidence/Investigation",
            $"{ArtRoot}/Props/Puzzles",
        };

        foreach (string folder in candidates)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                continue;
            }

            string[] contents = AssetDatabase.FindAssets(string.Empty, new[] { folder });
            if (contents.Length == 0)
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }
    }

    private static void ApplyHighQualityImportSettings()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtRoot });
        var changedPaths = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            {
                continue;
            }

            bool changed = false;
            changed |= SetIfDifferent(importer.mipmapEnabled, false, value => importer.mipmapEnabled = value);
            changed |= SetIfDifferent(importer.alphaIsTransparency, true, value => importer.alphaIsTransparency = value);
            changed |= SetIfDifferent(importer.npotScale, TextureImporterNPOTScale.None, value => importer.npotScale = value);
            changed |= SetIfDifferent(importer.filterMode, FilterMode.Bilinear, value => importer.filterMode = value);
            changed |= SetIfDifferent(importer.maxTextureSize, 4096, value => importer.maxTextureSize = value);
            changed |= SetIfDifferent(
                importer.textureCompression,
                TextureImporterCompression.Uncompressed,
                value => importer.textureCompression = value
            );

            if (changed)
            {
                changedPaths.Add(path);
                importer.SaveAndReimport();
            }
        }

        Debug.Log($"고화질 임포트 설정 갱신: {changedPaths.Count}개");
    }

    private static bool SetIfDifferent<T>(T current, T desired, Action<T> setter)
    {
        if (EqualityComparer<T>.Default.Equals(current, desired))
        {
            return false;
        }

        setter(desired);
        return true;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class AudioAssetMaintenance
{
    private const string AudioRoot = "Assets/_Project/Audio";

    private static readonly IReadOnlyDictionary<string, string> LegacyAudioMoves =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SFX_boat_engine_sound.mp3"] =
                "Ambience/Ship/AMB_ShipEngine.mp3",
            ["SFX_Cafe_buzzing_sound.mp3"] =
                "Ambience/Interior/AMB_CafeBuzz.mp3",
            ["SFX_factory_exhaust_fan_sound.mp3"] =
                "Ambience/Interior/AMB_ExhaustFan.mp3",
            ["SFX_sound_of_waves.mp3"] =
                "Ambience/Ocean/AMB_OceanWaves.mp3",
            ["SFX_Sound_of_waves_on_the_beach_1.mp3"] =
                "Ambience/Ocean/AMB_BeachSurf.mp3",
            ["SFX_wind_noise.mp3"] = "Ambience/Ship/AMB_Wind.mp3",
            ["SFX_Book Page Turning.mp3"] = "SFX/UI/SFX_BookPageTurn.mp3",
            ["SFX_boom_sound_effect.mp3"] = "SFX/StoryEvents/SFX_Boom.mp3",
            ["SFX_finger snap.mp3"] = "SFX/UI/SFX_FingerSnap.mp3",
            ["SFX_horn.mp3"] = "SFX/StoryEvents/SFX_ShipHorn.mp3",
            ["SFX_Mountain Hiking Footsteps.mp3"] =
                "SFX/Footsteps/SFX_MountainFootsteps.mp3",
            ["SFX_shoe_footsteps_sound_2.mp3"] =
                "SFX/Footsteps/SFX_ShoeFootsteps_02.mp3",
            ["SFX_sound_of_clipping_nails.mp3"] =
                "SFX/Puzzle/SFX_NailClipping.mp3",
            ["SFX_sound_of_water_sloshing.mp3"] =
                "SFX/Evidence/SFX_WaterSloshing.mp3",
            ["SFX_Suspicious.mp3"] =
                "SFX/StoryEvents/SFX_Suspicious.mp3",
            ["SFX_The_sound_of_an_iron_door_knocking.mp3"] =
                "SFX/Doors/SFX_IronDoorKnock.mp3",
            ["SFX_The_sound_of_an_iron_door_opening_and_closing.mp3"] =
                "SFX/Doors/SFX_IronDoorOpenClose.mp3",
            ["SFX_The_sound_of_flipping_a_calendar.mp3"] =
                "SFX/UI/SFX_CalendarFlip.mp3",
            ["SFX_The_sound_of_flipping_newspaper.mp3"] =
                "SFX/UI/SFX_NewspaperFlip.mp3",
            ["SFX_The_sound_of_water_splashing_in_a_bathtub.mp3"] =
                "SFX/StoryEvents/SFX_BathtubSplash.mp3",
            ["SFX_Type_Writer.mp3"] = "SFX/UI/SFX_Typewriter.mp3",
            ["SFX_Whoop_-_a_short_2.mp3"] = "SFX/UI/SFX_UIWhoop_02.mp3"
        };

    [MenuItem("Under the Horizon/Audio/역할별 폴더 정리")]
    public static void OrganizeByRuntimeRole()
    {
        foreach (KeyValuePair<string, string> move in LegacyAudioMoves)
        {
            MoveAsset(
                $"{AudioRoot}/SFX/{move.Key}",
                $"{AudioRoot}/{move.Value}");
        }

        OrganizeStoryRecordings();
        DeleteIfEmpty($"{AudioRoot}/VoiceBarks/story_recording");
        DeleteIfEmpty($"{AudioRoot}/SFX/Dubbing");
        DeleteIfEmpty($"{AudioRoot}/SFX/Machinery");
        DeleteIfEmpty($"{AudioRoot}/VoiceBarks/NPC");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Audio 자산을 런타임 버스 역할과 Story Scene 기준으로 정리했습니다.");
    }

    public static void RunFromCommandLine() => OrganizeByRuntimeRole();

    private static void OrganizeStoryRecordings()
    {
        const string sourceFolder = AudioRoot + "/VoiceBarks/story_recording";
        if (!AssetDatabase.IsValidFolder(sourceFolder))
            return;

        foreach (string guid in AssetDatabase.FindAssets(
                     "t:AudioClip",
                     new[] { sourceFolder }))
        {
            string source = AssetDatabase.GUIDToAssetPath(guid);
            string filename = Path.GetFileName(source);
            string destination = StoryRecordingDestination(filename);
            if (!string.IsNullOrEmpty(destination))
                MoveAsset(source, destination);
        }
    }

    private static string StoryRecordingDestination(string filename)
    {
        if (string.Equals(
                filename,
                "VO_DANIEL_DYING_R_01.mp3",
                StringComparison.OrdinalIgnoreCase))
        {
            return $"{AudioRoot}/StoryRecordings/D1_06/"
                + "REC_D1_06_DANIEL_DYING_R_01.mp3";
        }

        Match match = Regex.Match(
            filename ?? string.Empty,
            @"^VO_(D\d)-(\d{2})_(.+\.mp3)$",
            RegexOptions.IgnoreCase);
        if (!match.Success)
            return string.Empty;

        string sceneId = $"{match.Groups[1].Value}_{match.Groups[2].Value}";
        return $"{AudioRoot}/StoryRecordings/{sceneId}/"
            + $"REC_{sceneId}_{match.Groups[3].Value}";
    }

    private static void MoveAsset(string source, string destination)
    {
        if (AssetDatabase.LoadMainAssetAtPath(source) == null)
            return;
        if (AssetDatabase.LoadMainAssetAtPath(destination) != null)
        {
            throw new InvalidOperationException(
                $"Audio destination already exists: {destination}");
        }

        EnsureFolder(Path.GetDirectoryName(destination)?.Replace('\\', '/'));
        string error = AssetDatabase.MoveAsset(source, destination);
        if (!string.IsNullOrEmpty(error))
        {
            throw new InvalidOperationException(
                $"Audio asset move failed: {source} -> {destination}: {error}");
        }
    }

    private static void EnsureFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            return;
        string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }

    private static void DeleteIfEmpty(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return;
        if (AssetDatabase.FindAssets(string.Empty, new[] { folder }).Length == 0)
            AssetDatabase.DeleteAsset(folder);
    }
}

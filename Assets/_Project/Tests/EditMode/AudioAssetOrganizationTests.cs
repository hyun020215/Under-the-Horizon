using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class AudioAssetOrganizationTests
{
    private const string AudioRoot = "Assets/_Project/Audio";

    [Test]
    public void AudioClipsUseRoleFoldersAndPrefixes()
    {
        AssertRole("Music", "MUS_", 8);
        AssertRole("Ambience", "AMB_", 6);
        AssertRole("SFX", "SFX_", 16);
        AssertRole("VoiceBarks", "VO_", 227);
        AssertRole("StoryRecordings", "REC_", 16);
    }

    [Test]
    public void StoryRecordingsAreGroupedByCanonicalStoryScene()
    {
        string[] paths = FindClips("StoryRecordings");

        Assert.That(paths, Has.Length.GreaterThanOrEqualTo(16));
        Assert.That(
            paths.All(path => Regex.IsMatch(
                Path.GetFileName(Path.GetDirectoryName(path)) ?? string.Empty,
                @"^(P\d{2}|D\d_\d{2})$")),
            Is.True);
        Assert.That(
            AssetDatabase.IsValidFolder(
                $"{AudioRoot}/VoiceBarks/story_recording"),
            Is.False);
    }

    [Test]
    public void SfxRootContainsNoUnclassifiedAudioClips()
    {
        string[] rootClips = FindClips("SFX")
            .Where(path => string.Equals(
                Path.GetDirectoryName(path)?.Replace('\\', '/'),
                $"{AudioRoot}/SFX",
                StringComparison.Ordinal))
            .ToArray();

        Assert.That(rootClips, Is.Empty);
    }

    private static void AssertRole(
        string folder,
        string prefix,
        int expectedCount)
    {
        string[] paths = FindClips(folder);
        Assert.That(paths, Has.Length.GreaterThanOrEqualTo(expectedCount), folder);
        Assert.That(
            paths.All(path => Path.GetFileName(path).StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase)),
            Is.True,
            folder);
    }

    private static string[] FindClips(string folder) =>
        AssetDatabase.FindAssets(
                "t:AudioClip",
                new[] { $"{AudioRoot}/{folder}" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => AssetDatabase.LoadAssetAtPath<AudioClip>(path) != null)
            .ToArray();
}

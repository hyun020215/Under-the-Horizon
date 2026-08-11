using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class AccessibilityPresentationTests
{
    [Test]
    public void SettingsPrefabProvidesMotionAndDialogueSpeedControls()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(
            "Assets/_Project/Prefabs/UI/PF_SettingsScreen.prefab");
        try
        {
            Assert.That(root.transform.Find("움직임 줄이기 Toggle"), Is.Not.Null);
            Assert.That(root.transform.Find("대화 표시 속도 Dropdown"), Is.Not.Null);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    [Test]
    public void AccessibilityConsumersReadSharedServiceWithoutOwningPreferences()
    {
        string dialogue = File.ReadAllText("Assets/_Project/Runtime/UI/Screens/DialogueScreen.cs");
        string motion = File.ReadAllText("Assets/_Project/Runtime/Characters/CharacterIdleMotion.cs");
        string feedback = File.ReadAllText("Assets/_Project/Runtime/UI/Components/UiButtonFeedback.cs");
        Assert.That(dialogue, Does.Contain("AccessibilitySettingsService"));
        Assert.That(motion, Does.Contain("accessibility?.ReducedMotion"));
        Assert.That(feedback, Does.Contain("accessibility?.ReducedMotion"));
        Assert.That(dialogue + motion + feedback, Does.Not.Contain("PlayerPrefs"));
    }
}

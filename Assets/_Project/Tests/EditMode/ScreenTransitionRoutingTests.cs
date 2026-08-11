using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class ScreenTransitionRoutingTests
{
    [Test]
    public void GameRouterUsesAuthoredProfilesForFeatureScreens()
    {
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/Game.unity", OpenSceneMode.Single);
        ScreenRouter router = Object.FindFirstObjectByType<ScreenRouter>();
        Assert.That(router, Is.Not.Null);
        SerializedProperty routes = new SerializedObject(router).FindProperty("transitionRoutes");
        var mapped = new Dictionary<ScreenId, string>();
        for (var index = 0; index < routes.arraySize; index++)
        {
            SerializedProperty route = routes.GetArrayElementAtIndex(index);
            var id = (ScreenId)route.FindPropertyRelative("screen").enumValueIndex;
            var profile = (TransitionProfile)route.FindPropertyRelative("profile").objectReferenceValue;
            mapped[id] = profile?.name;
        }
        Assert.That(mapped[ScreenId.Investigation], Is.EqualTo("TRANS_INVESTIGATION_OPEN"));
        Assert.That(mapped[ScreenId.InvestigationRecord], Is.EqualTo("TRANS_DISCOVERY"));
        Assert.That(mapped[ScreenId.EvidenceBoard], Is.EqualTo("TRANS_DISCOVERY"));
        Assert.That(mapped[ScreenId.Puzzle], Is.EqualTo("TRANS_PUZZLE_OPEN"));
        Assert.That(mapped[ScreenId.Ending], Is.EqualTo("TRANS_ENDING"));
    }
}

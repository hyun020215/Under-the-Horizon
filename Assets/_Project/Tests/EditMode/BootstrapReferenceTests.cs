using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class BootstrapReferenceTests
{
    private const string GameDefinitionPath =
        "Assets/_Project/Content/Game/GAME_UnderTheHorizon.asset";

    [Test]
    public void BootstrapSceneReferencesCanonicalGameDefinition()
    {
        Scene scene = EditorSceneManager.OpenScene(
            "Assets/_Project/Scenes/Bootstrap.unity",
            OpenSceneMode.Additive);

        try
        {
            AppBootstrap bootstrap = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AppBootstrap>(true))
                .Single();
            SerializedObject serialized = new(bootstrap);

            Assert.That(
                serialized.FindProperty("gameDefinition").objectReferenceValue,
                Is.EqualTo(AssetDatabase.LoadAssetAtPath<GameDefinition>(GameDefinitionPath)));
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}

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
    private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
    private const string HotspotPrefabPath =
        "Assets/_Project/Prefabs/Interaction/PF_Hotspot.prefab";
    private const string CharacterHotspotPrefabPath =
        "Assets/_Project/Prefabs/Interaction/PF_CharacterHotspot.prefab";
    private const string CharacterViewPrefabPath =
        "Assets/_Project/Prefabs/Characters/PF_CharacterView.prefab";

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

    [Test]
    public void GameSceneWiresInteractionFlowHotspotsAndSaveCheckpoint()
    {
        Scene scene = EditorSceneManager.OpenScene(
            GameScenePath,
            OpenSceneMode.Additive);

        try
        {
            InteractionDirector interactions = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<InteractionDirector>(true))
                .Single();
            SerializedObject interactionData = new(interactions);
            InteractionPointView hotspot = AssetDatabase
                .LoadAssetAtPath<GameObject>(HotspotPrefabPath)
                .GetComponent<InteractionPointView>();

            Assert.That(
                interactionData.FindProperty("flow").objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                interactionData.FindProperty("hotspotPrefab").objectReferenceValue,
                Is.EqualTo(hotspot));
            Assert.That(
                interactionData.FindProperty("hotspotRoot").objectReferenceValue.name,
                Is.EqualTo("HotspotLayer"));

            GameStartup startup = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<GameStartup>(true))
                .Single();
            SaveCheckpoint checkpoint = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<SaveCheckpoint>(true))
                .Single();
            SerializedObject startupData = new(startup);
            SerializedObject checkpointData = new(checkpoint);

            Assert.That(
                startupData.FindProperty("saveCheckpoint").objectReferenceValue,
                Is.EqualTo(checkpoint));
            Assert.That(
                checkpointData.FindProperty("stateStore").objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                checkpointData.FindProperty("storyScenes").objectReferenceValue,
                Is.Not.Null);

            GameObject transitionOverlay = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Single(item => item.name == "TransitionOverlay")
                .gameObject;
            Assert.That(
                transitionOverlay.GetComponent<UnityEngine.UI.Image>().raycastTarget,
                Is.False);
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void CharacterPrefabWiresAnchoredContextViewAndTooltip()
    {
        GameObject characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            CharacterViewPrefabPath);
        GameObject contextPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            CharacterHotspotPrefabPath);

        Assert.That(characterPrefab, Is.Not.Null);
        Assert.That(contextPrefab, Is.Not.Null);

        CharacterView character = characterPrefab.GetComponent<CharacterView>();
        InteractionPointView context = contextPrefab.GetComponent<InteractionPointView>();
        Assert.That(character, Is.Not.Null);
        Assert.That(context, Is.Not.Null);
        var characterData = new SerializedObject(character);
        Assert.That(
            characterData.FindProperty("contextBadgePrefab").objectReferenceValue,
            Is.SameAs(context),
            "CharacterView must instantiate the canonical Context affordance prefab.");

        Assert.That(context.Tooltip, Is.Not.Null);
        Assert.That(
            context.Tooltip.transform.IsChildOf(context.transform),
            Is.True);
        Assert.That(
            context.GetComponent<UnityEngine.UI.Graphic>().raycastTarget,
            Is.True,
            "The visible Context affordance must receive pointer hover and click events.");
    }
}

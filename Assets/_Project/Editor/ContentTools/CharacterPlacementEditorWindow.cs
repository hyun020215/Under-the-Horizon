using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class CharacterPlacementEditorWindow
    : ContentAssetEditorWindow<CharacterPlacementSet>
{
    private int referenceResolutionIndex;

    [MenuItem("Under The Horizon/Content/Character Placements")]
    private static void Open() =>
        GetWindow<CharacterPlacementEditorWindow>("Placements");

    private void OnGUI() => DrawContent();

    protected override void DrawSelectedTools(CharacterPlacementSet set)
    {
        EditorGUILayout.LabelField(
            "Background-space authoring",
            EditorStyles.boldLabel);

        StorySceneDefinition[] scenes =
            CharacterPlacementAuthoringUtility.FindReferencingScenes(set);
        EditorGUILayout.LabelField(
            "Story Scenes",
            scenes.Length == 0
                ? "None"
                : string.Join(", ", scenes.Select(scene => scene.Id)));
        if (scenes.Length > 0
            && GUILayout.Button("Open first Story Scene preview"))
        {
            StoryScenePreviewWindow.OpenForScene(scenes[0]);
        }

        Vector2Int[] resolutions = VisualQaResolutionMatrix.Resolutions.ToArray();
        referenceResolutionIndex = EditorGUILayout.Popup(
            "Conversion reference",
            Mathf.Clamp(referenceResolutionIndex, 0, resolutions.Length - 1),
            resolutions.Select(FormatResolution).ToArray());
        Vector2Int referenceResolution = resolutions[referenceResolutionIndex];

        if (!CharacterPlacementAuthoringUtility.TryCreateConversionProposal(
                set,
                scenes,
                referenceResolution,
                resolutions,
                out CharacterPlacementConversionProposal proposal,
                out string error))
        {
            EditorGUILayout.HelpBox(error, MessageType.Warning);
            return;
        }

        EditorGUILayout.ObjectField(
            "Effective background",
            proposal.Background,
            typeof(Sprite),
            false);
        EditorGUILayout.LabelField(
            "Proposed coordinates",
            EditorStyles.miniBoldLabel);
        for (int index = 0; index < proposal.Before.Count; index++)
        {
            string character = set.Placements[index].character?.Id
                ?? $"Placement {index}";
            EditorGUILayout.LabelField(
                character,
                $"{proposal.Before[index]:F4} → {proposal.After[index]:F4}");
        }

        EditorGUILayout.HelpBox(
            "변환은 선택한 Set 하나의 좌표 공간과 X/Y만 하나의 Undo 단계로 변경합니다. "
            + "Scale·정렬·자세·표정·클릭 설정은 유지하며 자동 저장하지 않습니다.",
            MessageType.Info);
        if (GUILayout.Button(
                "Convert selected Set to BackgroundNormalized"))
        {
            CharacterPlacementAuthoringUtility.ApplyConversion(proposal);
        }
    }

    private static string FormatResolution(Vector2Int resolution) =>
        $"{resolution.x} × {resolution.y}";
}

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterPlacementSet))]
public sealed class CharacterPlacementSetInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField(
                "Script",
                MonoScript.FromScriptableObject(
                    (CharacterPlacementSet)target),
                typeof(MonoScript),
                false);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("placementSpace"));
        }

        EditorGUILayout.HelpBox(
            "Placement Space는 Character Placements 창의 검증된 변환 도구로만 "
            + "변경합니다. 좌표를 변환하지 않고 공간만 바꾸면 배치가 이동합니다.",
            MessageType.Info);
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("placements"),
            true);

        serializedObject.ApplyModifiedProperties();
    }
}

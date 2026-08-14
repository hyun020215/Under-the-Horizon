using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class P0ProjectBuilderPlacementTests
{
    private const string P02PlacementPath =
        "Assets/_Project/Content/Characters/PlacementSets/Generated/"
        + "SET_P_02_CHARACTERS.asset";

    [Test]
    public void RebuildPreservesExistingPlacementsByStableCharacterId()
    {
        CharacterPlacementSet set = AssetDatabase.LoadAssetAtPath<
            CharacterPlacementSet>(P02PlacementPath);
        Assert.That(set, Is.Not.Null);
        CharacterPlacementSet snapshot = UnityEngine.Object.Instantiate(set);
        snapshot.name = set.name;
        bool wasDirty = EditorUtility.IsDirty(set);

        try
        {
            InvokeSupportAssetBuild(
                sceneId: "P-02",
                characters: "이블린; 다니엘; 리처드");

            CharacterPlacement[] expected = snapshot.Placements;
            CharacterPlacement[] actual = set.Placements;
            Assert.That(actual, Has.Length.EqualTo(expected.Length));
            for (int index = 0; index < expected.Length; index++)
                AssertPlacementEqual(expected[index], actual[index], index);
        }
        finally
        {
            EditorUtility.CopySerialized(snapshot, set);
            if (!wasDirty)
                EditorUtility.ClearDirty(set);
            UnityEngine.Object.DestroyImmediate(snapshot);
        }
    }

    private static void InvokeSupportAssetBuild(
        string sceneId,
        string characters)
    {
        Type builderType = typeof(P0ProjectBuilder);
        Type rowType = builderType.GetNestedType(
            "SceneRow",
            BindingFlags.NonPublic);
        Assert.That(rowType, Is.Not.Null);

        object row = Activator.CreateInstance(rowType);
        rowType.GetField("Id")?.SetValue(row, sceneId);
        rowType.GetField("Characters")?.SetValue(row, characters);

        IList rows = (IList)Activator.CreateInstance(
            typeof(List<>).MakeGenericType(rowType));
        rows.Add(row);

        MethodInfo build = builderType.GetMethod(
            "BuildSceneSupportAssets",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(build, Is.Not.Null);
        build.Invoke(null, new object[] { rows });
    }

    private static void AssertPlacementEqual(
        CharacterPlacement expected,
        CharacterPlacement actual,
        int index)
    {
        string label = $"placement {index} ({expected.character?.Id})";
        Assert.That(actual.character, Is.SameAs(expected.character), label);
        Assert.That(actual.normalizedX, Is.EqualTo(expected.normalizedX), label);
        Assert.That(actual.normalizedY, Is.EqualTo(expected.normalizedY), label);
        Assert.That(actual.scale, Is.EqualTo(expected.scale), label);
        Assert.That(actual.sortingOrder, Is.EqualTo(expected.sortingOrder), label);
        Assert.That(actual.pose, Is.EqualTo(expected.pose), label);
        Assert.That(actual.expression, Is.EqualTo(expected.expression), label);
        Assert.That(actual.clickable, Is.EqualTo(expected.clickable), label);
    }
}

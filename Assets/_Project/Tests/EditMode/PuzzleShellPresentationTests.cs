using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class PuzzleShellPresentationTests
{
    [Test]
    public void PuzzlePrefabProvidesCommonGuidanceAndResultControls()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(
            "Assets/_Project/Prefabs/UI/PF_PuzzleScreen.prefab");
        try
        {
            Assert.That(root.transform.Find("Puzzle Frame/Puzzle Title"), Is.Not.Null);
            Assert.That(root.transform.Find("Puzzle Frame/Controller Workspace/Hint"), Is.Not.Null);
            Assert.That(root.transform.Find("Puzzle Frame/HintButton"), Is.Not.Null);
            Assert.That(root.transform.Find("Puzzle Frame/CancelButton"), Is.Not.Null);
            Assert.That(root.transform.Find("Puzzle Frame/ReturnButton"), Is.Not.Null);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    [Test]
    public void PuzzleShellLeavesCompletionEffectsOwnedByDirectorAndDefinition()
    {
        string screen = File.ReadAllText("Assets/_Project/Runtime/UI/Screens/PuzzleScreen.cs");
        string director = File.ReadAllText("Assets/_Project/Runtime/Puzzles/Core/PuzzleDirector.cs");
        Assert.That(screen, Does.Not.Contain("ApplyCompletion"));
        Assert.That(screen, Does.Not.Contain("CompletePuzzle"));
        Assert.That(director, Does.Contain("definition.ApplyCompletion(state)"));
        Assert.That(director, Does.Contain("puzzleScreen?.Present"));
    }
}

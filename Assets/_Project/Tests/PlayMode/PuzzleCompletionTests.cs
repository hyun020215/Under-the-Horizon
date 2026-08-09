using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class PuzzleCompletionTests
{
    [UnityTest]
    public IEnumerator BloodPatternCompletesAndAppliesLogicalPuzzleState()
    {
        var gameObject = new GameObject("PuzzleCompletionTest");
        PuzzleDefinition definition = ScriptableObject.CreateInstance<PuzzleDefinition>();

        try
        {
            typeof(PuzzleDefinition)
                .GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(definition, "PUZ_TEST");
            GameStateStore state = gameObject.AddComponent<GameStateStore>();
            BloodPatternPuzzleController controller =
                gameObject.AddComponent<BloodPatternPuzzleController>();
            var task = controller.PlayAsync(new PuzzleContext(definition, state));

            int[] pieces = { 7, 2, 5, 1, 8, 0, 6, 3, 4 };
            for (var target = 0; target < pieces.Length; target++)
            {
                int source = System.Array.IndexOf(pieces, target);
                controller.Swap(target, source);
                (pieces[target], pieces[source]) = (pieces[source], pieces[target]);
            }

            Assert.That(controller.Submit(), Is.True);
            while (!task.IsCompleted)
                yield return null;

            Assert.That(task.Result.Completed, Is.True);
            definition.ApplyCompletion(state);
            Assert.That(state.IsPuzzleCompleted("PUZ_TEST"), Is.True);
        }
        finally
        {
            Object.Destroy(definition);
            Object.Destroy(gameObject);
        }
    }
}

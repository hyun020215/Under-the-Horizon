using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class PuzzleRuleContentTests
{
    private GameObject host;
    private GameStateStore state;

    [SetUp]
    public void SetUp()
    {
        host = new GameObject(nameof(PuzzleRuleContentTests));
        state = host.AddComponent<GameStateStore>();
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(host);

    [Test]
    public void BloodPatternUsesMigratedSelectionsAndHints()
    {
        PuzzleDefinition definition = Load("PUZ_D2_02_BloodPattern");
        BloodPatternPuzzleController controller = host.AddComponent<BloodPatternPuzzleController>();
        var task = controller.PlayAsync(new PuzzleContext(definition, state));

        Assert.That(controller.SelectObservation("invented"), Is.False);
        foreach (string id in definition.Rules.SolutionIds)
            Assert.That(controller.SelectObservation(id), Is.True);
        Assert.That(controller.RequestHint(), Does.Contain("비산혈흔"));
        Assert.That(controller.SubmitAuthoredRule(), Is.True);
        Assert.That(task.Result.Completed, Is.True);
    }

    [Test]
    public void CargoRailRequiresAllCanonicalEvidence()
    {
        PuzzleDefinition definition = Load("PUZ_D6_02_CargoRail");
        CargoRailPuzzleController controller = host.AddComponent<CargoRailPuzzleController>();
        controller.PlayAsync(new PuzzleContext(definition, state));
        controller.SetRoute(definition.Rules.SolutionIds);

        state.AddEvidence("C-08");
        state.AddEvidence("C-09");
        Assert.That(controller.SubmitAuthoredRule(), Is.False);
        state.AddEvidence("C-10");
        Assert.That(controller.SubmitAuthoredRule(), Is.True);
    }

    [Test]
    public void MigratedRulesContainOnlyAllowedSolutions()
    {
        foreach (string name in new[] { "PUZ_D2_02_BloodPattern", "PUZ_D6_02_CargoRail" })
        {
            PuzzleDefinition definition = Load(name);
            Assert.That(definition.Rules, Is.Not.Null);
            Assert.That(definition.Rules.Hints, Has.Length.EqualTo(3));
            Assert.That(
                definition.Rules.SolutionIds.All(definition.Rules.AllowedInputIds.Contains),
                Is.True,
                name);
        }
    }

    [Test]
    public void UnmigratedPuzzleRulesRemainBackwardCompatible()
    {
        PuzzleDefinition definition = Load("PUZ_D2_04_CCTVLogs");
        Assert.That(definition.Rules, Is.Not.Null);
        Assert.That(definition.Rules.IsAuthored, Is.False);
    }

    private static PuzzleDefinition Load(string name) => AssetDatabase
        .FindAssets($"{name} t:PuzzleDefinition")
        .Select(AssetDatabase.GUIDToAssetPath)
        .Select(AssetDatabase.LoadAssetAtPath<PuzzleDefinition>)
        .Single();
}

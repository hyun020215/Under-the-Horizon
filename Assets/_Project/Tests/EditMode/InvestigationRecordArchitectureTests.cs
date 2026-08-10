using System.IO;
using NUnit.Framework;

public sealed class InvestigationRecordArchitectureTests
{
    [Test]
    public void RecordScreenRendersExistingEvidenceInventoryWithoutDirectStateMutation()
    {
        string source = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Screens/InvestigationRecordScreen.cs");
        Assert.That(source, Does.Contain("evidence?.Inventory?.Discovered"));
        Assert.That(source, Does.Not.Contain("AddEvidence"));
        Assert.That(source, Does.Not.Contain("discoveredEvidence"));
        Assert.That(source, Does.Not.Contain("DATABASE_Evidence"));
    }
}

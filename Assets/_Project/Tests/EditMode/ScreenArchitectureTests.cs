using System.IO;
using NUnit.Framework;

public sealed class ScreenArchitectureTests
{
    [Test]
    public void ScreenBaseDoesNotOwnTransitionAnimation()
    {
        string source = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Core/ScreenBase.cs");
        Assert.That(source, Does.Not.Contain("CanvasGroup"));
        Assert.That(source, Does.Not.Contain("Time.realtimeSinceStartup"));
        Assert.That(source, Does.Not.Contain("Animate("));
    }

    [Test]
    public void RouterDefaultPathUsesTransitionDirectorAndProfile()
    {
        string source = File.ReadAllText(
            "Assets/_Project/Runtime/UI/Core/ScreenRouter.cs");
        Assert.That(source, Does.Contain("transitionDirector"));
        Assert.That(source, Does.Contain("defaultTransition"));
        Assert.That(source,
            Does.Contain("OpenAsync(id, context, transitionDirector, ResolveTransition(id))"));
        Assert.That(source, Does.Contain("return defaultTransition"));
    }
}

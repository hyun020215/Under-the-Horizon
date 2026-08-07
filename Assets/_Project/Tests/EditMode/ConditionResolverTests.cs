using NUnit.Framework;

public sealed class ConditionResolverTests
{
    [Test]
    public void EmptyConditionsPass() => Assert.IsTrue(ConditionResolver.All(null, null));
}

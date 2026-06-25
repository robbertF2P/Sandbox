using Reference.Domain;

namespace Reference.Unit.Tests;

[Trait("Module", "Reference")]
[Trait("Tier", "Unit")]
public sealed class ReferenceStatusRulesTests
{
    [Fact]
    public void ResolveHealth_WhenModuleRegisteredAndNoAdapter_ReturnsHealthy()
    {
        ReferenceHealth health = ReferenceStatusRules.ResolveHealth(
            moduleRegistered: true,
            adapterPresent: false);

        Assert.Equal(ReferenceHealth.Healthy, health);
    }

    [Fact]
    public void ResolveHealth_WhenAdapterPresent_ReturnsDegraded()
    {
        ReferenceHealth health = ReferenceStatusRules.ResolveHealth(
            moduleRegistered: true,
            adapterPresent: true);

        Assert.Equal(ReferenceHealth.Degraded, health);
    }

    [Fact]
    public void ResolveHealth_WhenModuleNotRegistered_ReturnsUnknown()
    {
        ReferenceHealth health = ReferenceStatusRules.ResolveHealth(
            moduleRegistered: false,
            adapterPresent: false);

        Assert.Equal(ReferenceHealth.Unknown, health);
    }
}

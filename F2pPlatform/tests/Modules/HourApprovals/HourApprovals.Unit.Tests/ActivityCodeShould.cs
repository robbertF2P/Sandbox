using Platform.Shared.Domain;

namespace HourApprovals.Unit.Tests;

[Trait("Module", "HourApprovals")]
[Trait("Tier", "Unit")]
public sealed class ActivityCodeShould
{
    [Fact]
    public void Constructor_TrimsValue()
    {
        var code = new ActivityCode("  ACT-204-WIR  ");

        Assert.Equal("ACT-204-WIR", code.Value);
    }

    [Fact]
    public void Constructor_RejectsBlank()
    {
        Assert.Throws<ArgumentException>(() => new ActivityCode("   "));
    }
}

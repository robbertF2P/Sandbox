using AkkaSignalRVuePoc.Data.Values;

namespace AkkaSignalRVuePoc.Api.Tests.Data;

public sealed class HoursTests
{
    [Theory]
    [InlineData(0f)]
    [InlineData(1e-7f)]
    [InlineData(-1e-7f)]
    [InlineData(5e-7f)]
    public void FromDatabase_NormalizesNearZeroToTrueZero(float storedValue)
    {
        var hours = Hours.FromDatabase(storedValue);

        Assert.True(hours.IsZero);
        Assert.Equal(0f, hours.Value);
    }

    [Theory]
    [InlineData(0.000001f)]
    [InlineData(-0.000001f)]
    [InlineData(1.5f)]
    public void FromDatabase_PreservesMeaningfulValues(float storedValue)
    {
        var hours = Hours.FromDatabase(storedValue);

        Assert.Equal(Hours.Normalize(storedValue), hours.Value);
        Assert.False(hours.IsZero);
    }

    [Fact]
    public void FromHours_NormalizesOnWrite()
    {
        var hours = Hours.FromHours(2.5f - 2.5f);

        Assert.True(hours.IsZero);
    }

    [Fact]
    public void Equality_UsesNormalizedValues()
    {
        var fromDatabase = Hours.FromDatabase(1e-8f);
        var fromHours = Hours.FromHours(0f);

        Assert.Equal(fromHours, fromDatabase);
    }
}

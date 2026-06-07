using AkkaSignalRVuePoc.Data.Values;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AkkaSignalRVuePoc.Data.Conversions;

public sealed class HoursValueConverter : ValueConverter<Hours, float>
{
  public static HoursValueConverter Instance { get; } = new();

  public HoursValueConverter()
    : base(
      hours => hours.Value,
      stored => Hours.FromDatabase(stored))
  {
  }
}

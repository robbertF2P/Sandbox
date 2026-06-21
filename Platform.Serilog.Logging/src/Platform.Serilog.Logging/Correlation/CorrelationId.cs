namespace Platform.Serilog.Logging.Correlation;

public static class CorrelationId
{
    public static string New() => Guid.NewGuid().ToString("N");

    public static string GetOrCreate(string? candidate) =>
        string.IsNullOrWhiteSpace(candidate) ? New() : candidate.Trim();

    public static bool TryParse(string? value, out string correlationId)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            correlationId = string.Empty;
            return false;
        }

        correlationId = value.Trim();
        return true;
    }
}

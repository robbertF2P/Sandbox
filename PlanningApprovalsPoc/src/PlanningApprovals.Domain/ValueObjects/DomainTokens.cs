namespace PlanningApprovals.Domain.ValueObjects;

public readonly record struct CorrelationId
{
    public string Value { get; }

    public CorrelationId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Correlation id is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}

public readonly record struct ProcessName
{
    public string Value { get; }

    public ProcessName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Process name is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}

public readonly record struct CaptureSource
{
    public string Value { get; }

    public CaptureSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Capture source is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}

public readonly record struct ProgressSource
{
    public string Value { get; }

    public ProgressSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Progress source is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public static ProgressSource Timesheet { get; } = new("Timesheet");

    public override string ToString() => Value;
}

public readonly record struct CalculationRunId
{
    public string Value { get; }

    public CalculationRunId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Calculation run id is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}

public readonly record struct ProfileFingerprint
{
    public string Value { get; }

    public ProfileFingerprint(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Profile fingerprint is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}

public readonly record struct ScopeDescription
{
    public string Value { get; }

    public ScopeDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Scope description is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}

public readonly record struct DecisionComment
{
    public string Value { get; }

    public DecisionComment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Decision comment cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}

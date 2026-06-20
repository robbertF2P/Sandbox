namespace ImportPipeline.Domain;

/// <summary>
/// Discriminated union for the outcome of mapping an entire ImportRow.
///
/// Skipped — at least one SkipIfEmpty rule fired; discard the row.
/// Invalid — at least one required field was absent; report which ones.
/// Mapped  — all rules satisfied; values ready for the persistence layer.
///
/// Priority: Skipped > Invalid > Mapped.
/// </summary>
public abstract record RowMappingResult
{
    public sealed record Skipped(string Reason) : RowMappingResult;
    public sealed record Invalid(IReadOnlyList<string> MissingFields) : RowMappingResult;
    public sealed record Mapped(IReadOnlyList<FieldValue> Values) : RowMappingResult;
}

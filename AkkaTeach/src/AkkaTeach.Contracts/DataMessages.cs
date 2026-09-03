namespace AkkaTeach.Contracts;

/// <summary>
/// A single record returned by the external data API.
/// </summary>
public sealed record ExternalDataRecord(string Id, int Value, string Source);

/// <summary>
/// One page of results from a paginated API.
/// </summary>
public sealed record ApiDataPage(int PageNumber, int TotalPages, IReadOnlyList<ExternalDataRecord> Records);

/// <summary>
/// Starts collecting and processing all pages from the API client.
/// </summary>
public sealed record CollectDataCommand(string? CollectionId = null) : IActorSystemMessage;

/// <summary>
/// Command routed to a worker in the pool for a single record.
/// </summary>
public sealed record ProcessDataRecordCommand(ExternalDataRecord Record) : IActorSystemMessage;

/// <summary>
/// Reply after a worker has processed one record.
/// </summary>
public sealed record DataRecordProcessed(string RecordId, int ProcessedValue) : IActorSystemMessage;

/// <summary>
/// Query for ingestion progress.
/// </summary>
public sealed record GetIngestionStatusQuery : IActorSystemMessage;

/// <summary>
/// Current ingestion state and counters.
/// </summary>
public sealed record IngestionStatusResponse(
    string State,
    int PagesCollected,
    int RecordsProcessed,
    int TotalRecords) : IActorSystemMessage;

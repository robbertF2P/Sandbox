namespace AkkaTeach.Contracts;

/// <summary>
/// Starts an async fetch. The actor uses <c>PipeTo</c> so the mailbox is not blocked while waiting.
/// </summary>
public sealed record FetchQuoteCommand(string Topic) : IActorSystemMessage;

/// <summary>
/// Reply after a quote has been fetched.
/// </summary>
public sealed record QuoteFetchedResponse(string Topic, string Quote) : IActorSystemMessage;

/// <summary>
/// Query whether the actor is idle or currently waiting on an async call.
/// </summary>
public sealed record GetFetchStatusQuery : IActorSystemMessage;

/// <summary>
/// <c>Idle</c> or <c>Fetching</c>.
/// </summary>
public sealed record FetchStatusResponse(string State) : IActorSystemMessage;

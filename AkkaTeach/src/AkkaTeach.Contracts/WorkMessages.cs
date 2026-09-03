namespace AkkaTeach.Contracts;

/// <summary>
/// Command to process a work item. The coordinator forwards this to a child worker.
/// </summary>
public sealed record ProcessWorkItemCommand(string ItemId, int Payload) : IActorSystemMessage;

/// <summary>
/// Reply from a worker after processing a single item.
/// </summary>
public sealed record WorkItemProcessed(string ItemId, int Result) : IActorSystemMessage;

/// <summary>
/// Query for how many items the coordinator has completed.
/// </summary>
public sealed record GetCompletedCountQuery : IActorSystemMessage;

/// <summary>
/// Response to <see cref="GetCompletedCountQuery"/>.
/// </summary>
public sealed record CompletedCountResponse(int Count) : IActorSystemMessage;

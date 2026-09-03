namespace AkkaTeach.Contracts;

public sealed record ActorSystemStarted(string ActorName) : IActorSystemEvent;

public sealed record WorkItemCompleted(string ItemId, int Result) : IActorSystemEvent;

public sealed record SessionStarted(string SessionId) : IActorSystemEvent;

public sealed record SessionEnded(string SessionId, int TotalSteps) : IActorSystemEvent;

public sealed record DataCollectionStarted(string CollectionId, int TotalPages, int ExpectedRecords) : IActorSystemEvent;

public sealed record DataPageProcessed(int PageNumber, int RecordCount) : IActorSystemEvent;

public sealed record DataCollectionCompleted(string CollectionId, int TotalRecords, long ElapsedMilliseconds) : IActorSystemEvent;

public sealed record PeerMessageDelivered(string To, string From, string Text) : IActorSystemEvent;

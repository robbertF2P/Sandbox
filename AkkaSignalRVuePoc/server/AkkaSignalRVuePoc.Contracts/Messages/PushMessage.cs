namespace AkkaSignalRVuePoc.Contracts.Messages;

public sealed record PushMessage(
    long Sequence,
    string Text,
    DateTimeOffset SentAt,
    string Source);

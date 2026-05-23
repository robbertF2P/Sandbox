namespace AkkaSignalRVuePoc.Api.Models;

public sealed record PushMessage(
    long Sequence,
    string Text,
    DateTimeOffset SentAt,
    string Source);

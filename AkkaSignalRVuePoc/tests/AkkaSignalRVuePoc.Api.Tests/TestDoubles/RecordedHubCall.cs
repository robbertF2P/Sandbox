namespace AkkaSignalRVuePoc.Api.Tests.TestDoubles;

internal sealed record RecordedHubCall(string Method, object?[] Arguments);

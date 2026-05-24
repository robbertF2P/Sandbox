namespace AkkaSignalRVuePoc.Contracts.Messages;

public sealed record PublishLiveMessageCommand(string Text) : IActorSystemMessage;

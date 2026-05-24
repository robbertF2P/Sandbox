namespace AkkaSignalRVuePoc.Contracts.Messages;

public sealed record PublishActorMessage(PushMessage Message) : IActorSystemMessage;

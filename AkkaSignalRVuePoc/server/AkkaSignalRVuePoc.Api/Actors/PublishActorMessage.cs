using AkkaSignalRVuePoc.Api.Models;

namespace AkkaSignalRVuePoc.Api.Actors;

public sealed record PublishActorMessage(PushMessage Message);

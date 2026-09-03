namespace AkkaTeach.Contracts;

/// <summary>
/// Send a message to a previously introduced peer by name.
/// </summary>
public sealed record SendPeerMessageCommand(string TargetName, string Text) : IActorSystemMessage;

/// <summary>
/// Delivered when another peer sends a chat message.
/// </summary>
public sealed record PeerMessageReceived(string From, string Text) : IActorSystemMessage;

/// <summary>
/// Lists peer names this actor knows how to reach.
/// </summary>
public sealed record GetKnownPeersQuery : IActorSystemMessage;

public sealed record KnownPeersResponse(IReadOnlyList<string> PeerNames) : IActorSystemMessage;

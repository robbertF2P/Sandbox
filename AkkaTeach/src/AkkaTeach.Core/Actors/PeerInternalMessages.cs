using Akka.Actor;

namespace AkkaTeach.Core.Actors;

/// <summary>
/// Registers another actor's address so this peer can message it later.
/// </summary>
internal sealed record IntroducePeerCommand(string PeerName, IActorRef PeerRef);

/// <summary>
/// Command to wire up a full peer network — every actor learns every other actor's address.
/// </summary>
internal sealed record WirePeerNetworkCommand(IReadOnlyList<PeerRegistration> Peers);

/// <summary>
/// Name + address pair used when bootstrapping a peer network.
/// </summary>
public sealed record PeerRegistration(string Name, IActorRef Ref);

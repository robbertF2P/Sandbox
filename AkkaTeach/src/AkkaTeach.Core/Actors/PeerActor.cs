using Akka.Actor;
using Akka.Event;
using AkkaTeach.Contracts;

namespace AkkaTeach.Core.Actors;

/// <summary>
/// A peer in the actor network. Once introduced to other peers, it can message any of them directly.
/// </summary>
/// <remarks>
/// <para><b>Key idea:</b> In Akka, <em>any actor can message any other actor</em> — there is no
/// requirement that they are parent/child. You only need the other actor's <see cref="IActorRef"/>.</para>
/// <para>This actor keeps a simple address book (<c>name → IActorRef</c>). After introduction,
/// <c>SendPeerMessageCommand</c> uses <c>Tell</c> to reach a peer directly across the system.</para>
/// </remarks>
public sealed class PeerActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly string _name;
    private readonly Dictionary<string, IActorRef> _peers = new(StringComparer.OrdinalIgnoreCase);

    public PeerActor(string name)
    {
        _name = name;

        Receive<IntroducePeerCommand>(command =>
        {
            _peers[command.PeerName] = command.PeerRef;
            _log.Debug("{Name} now knows peer {Peer} at {Path}", _name, command.PeerName, command.PeerRef.Path);
        });

        Receive<SendPeerMessageCommand>(command =>
        {
            if (!_peers.TryGetValue(command.TargetName, out var peer))
            {
                _log.Warning("{Name} does not know peer {Target}", _name, command.TargetName);
                if (!Sender.IsNobody())
                {
                    Sender.Tell(new KnownPeersResponse(_peers.Keys.ToList()));
                }

                return;
            }

            _log.Info("{From} -> {To}: {Text}", _name, command.TargetName, command.Text);
            peer.Tell(new PeerMessageReceived(_name, command.Text));
        });

        Receive<PeerMessageReceived>(message =>
        {
            _log.Info("{Name} received from {From}: {Text}", _name, message.From, message.Text);
            Context.System.EventStream.Publish(new PeerMessageDelivered(_name, message.From, message.Text));
        });

        Receive<GetKnownPeersQuery>(_ =>
            Sender.Tell(new KnownPeersResponse(_peers.Keys.OrderBy(n => n).ToList())));
    }

    public static Props Props(string name) => Akka.Actor.Props.Create(() => new PeerActor(name));
}

/// <summary>
/// One-shot helper that introduces every peer to every other peer (full mesh).
/// </summary>
/// <remarks>
/// Shows how actors get each other's addresses at startup — after this, they communicate peer-to-peer
/// with no coordinator in the message path.
/// </remarks>
public sealed class PeerIntroducerActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public PeerIntroducerActor()
    {
        Receive<WirePeerNetworkCommand>(command =>
        {
            _log.Info("Wiring peer network with {Count} actors", command.Peers.Count);

            foreach (var peer in command.Peers)
            {
                foreach (var other in command.Peers)
                {
                    if (peer.Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    peer.Ref.Tell(new IntroducePeerCommand(other.Name, other.Ref));
                }
            }
        });
    }

    public static Props Props() => Akka.Actor.Props.Create<PeerIntroducerActor>();

    /// <summary>
    /// Wires a peer network from tests or bootstrap code outside the actor system.
    /// </summary>
    public static void WireNetwork(IActorRef introducer, params PeerRegistration[] peers) =>
        introducer.Tell(new WirePeerNetworkCommand(peers));

    /// <summary>
    /// Introduces every peer to every other peer directly (used in tests to avoid timing issues).
    /// </summary>
    public static void WirePeersDirectly(params PeerRegistration[] peers)
    {
        foreach (var peer in peers)
        {
            foreach (var other in peers)
            {
                if (peer.Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                peer.Ref.Tell(new IntroducePeerCommand(other.Name, other.Ref));
            }
        }
    }
}

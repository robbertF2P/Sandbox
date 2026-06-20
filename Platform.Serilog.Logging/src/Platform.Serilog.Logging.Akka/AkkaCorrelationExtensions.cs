using Akka.Actor;
using Platform.Serilog.Logging.Correlation;

namespace Platform.Serilog.Logging.Akka;

public abstract class PlatformReceiveActor : ReceiveActor
{
    private readonly Dictionary<Type, SyncHandler> _syncHandlers = new();
    private readonly Dictionary<Type, AsyncHandler> _asyncHandlers = new();
    private bool _envelopeHandlerRegistered;

    private delegate void SyncHandler(object message, CorrelationFlow flow);

    private delegate Task AsyncHandler(object message, CorrelationFlow flow);

    protected void ReceiveCorrelated<TMessage>(Action<TMessage, CorrelationFlow> handler)
    {
        _syncHandlers[typeof(TMessage)] = (message, flow) => handler((TMessage)message, flow);
        EnsureEnvelopeHandlerRegistered();
    }

    protected void ReceiveCorrelated<TMessage>(Action<TMessage, CorrelationFlow, IActorRef> handler)
    {
        _syncHandlers[typeof(TMessage)] = (message, flow) => handler((TMessage)message, flow, Sender);
        EnsureEnvelopeHandlerRegistered();
    }

    protected void ReceiveCorrelatedAsync<TMessage>(Func<TMessage, CorrelationFlow, Task> handler)
    {
        _asyncHandlers[typeof(TMessage)] = (message, flow) => handler((TMessage)message, flow);
        EnsureEnvelopeHandlerRegistered();
    }

    protected void ReceiveCorrelatedAsync<TMessage>(Func<TMessage, Task> handler) =>
        ReceiveCorrelatedAsync<TMessage>(async (message, _) => await handler(message));

    protected void RegisterEnvelopeHandler()
    {
        _envelopeHandlerRegistered = false;
        EnsureEnvelopeHandlerRegistered();
    }

    private void EnsureEnvelopeHandlerRegistered()
    {
        if (_envelopeHandlerRegistered)
        {
            return;
        }

        _envelopeHandlerRegistered = true;
        ReceiveAsync<CorrelatedMessageEnvelope>(DispatchEnvelopeAsync);
    }

    private async Task DispatchEnvelopeAsync(CorrelatedMessageEnvelope envelope)
    {
        if (TryDispatchSync(envelope))
        {
            return;
        }

        if (await TryDispatchAsync(envelope))
        {
            return;
        }

        Unhandled(envelope);
    }

    private bool TryDispatchSync(CorrelatedMessageEnvelope envelope)
    {
        if (!_syncHandlers.TryGetValue(envelope.Message.GetType(), out SyncHandler? handler))
        {
            return false;
        }

        var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
        using CorrelationScope scope = flow.BeginScope();
        handler(envelope.Message, flow);
        return true;
    }

    private async Task<bool> TryDispatchAsync(CorrelatedMessageEnvelope envelope)
    {
        if (!_asyncHandlers.TryGetValue(envelope.Message.GetType(), out AsyncHandler? handler))
        {
            return false;
        }

        var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
        using CorrelationScope scope = flow.BeginScope();
        await handler(envelope.Message, flow);
        return true;
    }
}

public static class ActorRefCorrelationExtensions
{
    public static void TellCorrelated(
        this IActorRef actor,
        object message,
        string? useCase = null,
        IActorRef? sender = null)
    {
        CorrelationFlow flow = CorrelationFlow.FromCurrentOrNew(useCase);
        using CorrelationScope scope = flow.BeginScope();
        CorrelatedMessageEnvelope envelope = flow.Wrap(message);
        if (sender is null)
        {
            actor.Tell(envelope);
            return;
        }

        actor.Tell(envelope, sender);
    }

    public static async Task<TResponse> AskCorrelated<TResponse>(
        this IActorRef actor,
        object message,
        string useCase,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        CorrelationFlow flow = CorrelationFlow.FromCurrentOrNew(useCase);
        using CorrelationScope scope = flow.BeginScope();
        CorrelatedMessageEnvelope envelope = flow.Wrap(message);
        return await actor.Ask<TResponse>(envelope, timeout, cancellationToken);
    }
}

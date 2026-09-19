using Akka.Actor;
using Infrastructure.Akka.Actors.Workers;
using System;
using System.Collections.Generic;

namespace Infrastructure.Akka.Actors.Orchestration
{
    /// <summary>
    /// Drains a work queue with bounded parallelism. Replies once when every item has succeeded or failed.
    /// </summary>
    public sealed class BatchOrchestratorActor<TWorkItem> : ReplyTargetWorkerActor
    {
        private readonly BatchOrchestratorOptions<TWorkItem> _options;
        private readonly BatchOrchestratorContext _context = new();
        private readonly Dictionary<IActorRef, TWorkItem> _inFlight = new();

        private Queue<TWorkItem> _pending = new();
        private int _activeWorkers;

        public BatchOrchestratorActor(BatchOrchestratorOptions<TWorkItem> options)
        {
            _options = options;
            Receive<Start>(StartBatch);
            ReceiveAny(HandleWorkerMessage);
        }

        public static Props Props(BatchOrchestratorOptions<TWorkItem> options)
        {
            return global::Akka.Actor.Props.Create(() => new BatchOrchestratorActor<TWorkItem>(options));
        }

        public sealed record Start(IReadOnlyList<TWorkItem> Items, IActorRef ReplyTo);

        private void StartBatch(Start message)
        {
            Begin(message.ReplyTo);
            _pending = new Queue<TWorkItem>(message.Items);
            _context.Errors.Clear();
            _inFlight.Clear();
            _activeWorkers = 0;
            PumpWorkers();
        }

        private void HandleWorkerMessage(object message)
        {
            if (!_inFlight.TryGetValue(Sender, out var workItem))
            {
                return;
            }

            if (_options.IsSuccess(workItem, message))
            {
                _options.OnSuccess(workItem, message, _context);
                FinishWorker(Sender, workItem);
                return;
            }

            if (_options.IsFailure(workItem, message))
            {
                _options.OnFailure(workItem, message, _context);
                FinishWorker(Sender, workItem);
            }
        }

        private void FinishWorker(IActorRef worker, TWorkItem workItem)
        {
            _inFlight.Remove(worker);
            _activeWorkers--;
            Context.Unwatch(worker);
            PumpWorkers();
        }

        private void PumpWorkers()
        {
            while (_activeWorkers < Math.Max(1, _options.MaxConcurrency) && _pending.Count > 0)
            {
                var item = _pending.Dequeue();
                var (props, workerMessage) = _options.CreateWorker(item);
                var worker = Context.ActorOf(props);
                Context.Watch(worker);
                _inFlight[worker] = item;
                _activeWorkers++;
                worker.Tell(workerMessage);
            }

            if (_activeWorkers == 0 && _pending.Count == 0)
            {
                Complete(_options.BuildReply(_context));
            }
        }
    }
}

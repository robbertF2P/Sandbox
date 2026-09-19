using Akka.Actor;
using System;
using System.Collections.Generic;

namespace Infrastructure.Akka.Actors.Orchestration
{
    public sealed class BatchOrchestratorOptions<TWorkItem>
    {
        public int MaxConcurrency { get; init; } = 1;

        public required Func<TWorkItem, (Props Props, object Message)> CreateWorker { get; init; }

        public required Func<TWorkItem, object, bool> IsSuccess { get; init; }

        public required Func<TWorkItem, object, bool> IsFailure { get; init; }

        public required Action<TWorkItem, object, BatchOrchestratorContext> OnSuccess { get; init; }

        public required Action<TWorkItem, object, BatchOrchestratorContext> OnFailure { get; init; }

        public required Func<BatchOrchestratorContext, object> BuildReply { get; init; }
    }
}

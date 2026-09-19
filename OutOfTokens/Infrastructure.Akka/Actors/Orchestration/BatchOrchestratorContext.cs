using System.Collections.Generic;

namespace Infrastructure.Akka.Actors.Orchestration
{
    public sealed class BatchOrchestratorContext
    {
        public List<string> Errors { get; } = new();
    }
}

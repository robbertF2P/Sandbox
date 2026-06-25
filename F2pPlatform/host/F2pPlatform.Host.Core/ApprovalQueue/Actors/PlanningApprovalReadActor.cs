using Akka.Actor;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Planning;
using F2pPlatform.Host.Core.ApprovalQueue.Poc;

namespace F2pPlatform.Host.Core.ApprovalQueue.Actors;

/// <summary>
/// Planning module read boundary (POC). Answers with assignment rows from Planning's own store.
/// </summary>
public sealed class PlanningApprovalReadActor : ReceiveActor
{
    public PlanningApprovalReadActor()
    {
        Receive<GetPlanningAssignments>(message =>
        {
            Sender.Tell(new GetPlanningAssignmentsReply(PocPlanningAssignmentStore.ListAll()));
        });
    }

    public static Props Props() => Akka.Actor.Props.Create(() => new PlanningApprovalReadActor());
}

using Akka.Actor;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Timekeeping;
using F2pPlatform.Host.Core.ApprovalQueue.Poc;

namespace F2pPlatform.Host.Core.ApprovalQueue.Actors;

/// <summary>
/// Timekeeping module read boundary (POC). Answers with hours worked in the selected window.
/// </summary>
public sealed class TimekeepingApprovalReadActor : ReceiveActor
{
    public TimekeepingApprovalReadActor()
    {
        Receive<GetHoursInWindow>(message =>
        {
            IReadOnlyDictionary<Guid, decimal> hours =
                PocTimekeepingHoursStore.GetHours(message.AssignmentIds);

            Sender.Tell(new GetHoursInWindowReply(hours));
        });
    }

    public static Props Props() => Akka.Actor.Props.Create(() => new TimekeepingApprovalReadActor());
}

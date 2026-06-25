using F2pPlatform.Host.Contracts.ApprovalQueue;

namespace F2pPlatform.Host.Contracts.ApprovalQueue.Messages;

public sealed record GetApprovalQueue(ApprovalQueueFilter Filter);

public sealed record GetApprovalQueueReply(IReadOnlyList<ApprovalQueueRow> Rows);

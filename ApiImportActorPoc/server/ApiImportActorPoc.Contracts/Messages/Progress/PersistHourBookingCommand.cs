using ApiImportActorPoc.Contracts.Messages;
using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Messages.Progress;

public sealed record PersistHourBookingCommand(
    Guid ProcessingId,
    int AssignmentId,
    Hours Hours,
    string? Notes) : IActorSystemMessage;

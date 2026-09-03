namespace AkkaTeach.Contracts;

/// <summary>
/// Starts a teaching session. Only accepted while the session actor is idle.
/// </summary>
public sealed record StartSessionCommand(string SessionId) : IActorSystemMessage;

/// <summary>
/// Records progress during an active session.
/// </summary>
public sealed record RecordProgressCommand(int Step) : IActorSystemMessage;

/// <summary>
/// Ends the current session. Only accepted while active.
/// </summary>
public sealed record EndSessionCommand : IActorSystemMessage;

/// <summary>
/// Resets a completed session back to idle so a new session can begin.
/// </summary>
public sealed record ResetSessionCommand : IActorSystemMessage;

/// <summary>
/// Query for the current session state name.
/// </summary>
public sealed record GetSessionStateQuery : IActorSystemMessage;

/// <summary>
/// Response describing the session actor's current state.
/// </summary>
public sealed record SessionStateResponse(string State, string? SessionId, int StepsRecorded) : IActorSystemMessage;

namespace AkkaTeach.Contracts;

/// <summary>
/// Message sent to the greeter actor.
/// </summary>
public sealed record SayHelloCommand(string Name) : IActorSystemMessage;

/// <summary>
/// Reply from the greeter.
/// </summary>
public sealed record HelloReply(string Message) : IActorSystemMessage;

/// <summary>
/// Ask the front desk to greet someone using <c>Tell(target, message, sender)</c>.
/// </summary>
public sealed record AskViaTellCommand(string Name) : IActorSystemMessage;

/// <summary>
/// Ask the front desk to greet someone using <c>Forward(message)</c>.
/// </summary>
public sealed record AskViaForwardCommand(string Name) : IActorSystemMessage;

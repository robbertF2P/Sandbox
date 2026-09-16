namespace Floor2Plan.Connectors.P6.Actors;

/// <summary>Login succeeded — carries only the new session id.</summary>
internal sealed record P6LoginSucceeded(string SessionId);

/// <summary>Login failed — carries only the failure reason.</summary>
internal sealed record P6LoginFailed(string Reason);

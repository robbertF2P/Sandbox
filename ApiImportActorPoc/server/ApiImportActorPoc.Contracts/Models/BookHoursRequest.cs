using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models;

public sealed record BookHoursRequest(Hours Hours, string? Notes);

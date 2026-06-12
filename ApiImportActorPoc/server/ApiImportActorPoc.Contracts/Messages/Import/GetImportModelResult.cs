using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Contracts.Messages.Import;

public sealed record GetImportModelResult(bool Found, ProjectModel? Model);

namespace ApiImportActorPoc.Contracts.Models;

public enum ActivityRelationType
{
    Child = 0,
    Predecessor = 1,
    Successor = 2,
    FinishToStart = 3,
    StartToStart = 4,
    FinishToFinish = 5,
    StartToFinish = 6
}

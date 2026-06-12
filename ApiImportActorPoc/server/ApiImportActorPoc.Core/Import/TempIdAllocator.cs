namespace ApiImportActorPoc.Core.Import;

internal sealed class TempIdAllocator
{
    private int _next;

    public int Next() => --_next;
}

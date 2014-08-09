namespace DrivenIt.Foundation.Contracts.UnitOfWork
{
    public interface IUowFactory
    {
        IUow StartUnitOfWork(params ISupportUow[] supporters);
    }
}
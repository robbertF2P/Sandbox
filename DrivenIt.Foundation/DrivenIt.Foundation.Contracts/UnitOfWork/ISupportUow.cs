namespace DrivenIt.Foundation.Contracts.UnitOfWork
{
    /// <summary>
    /// Implementers should accept new unit of works and revert to the previous one on reset
    /// </summary>
    public interface ISupportUow
    {
        void Set(IUow unitOfWork);
        void Reset();
    }
}
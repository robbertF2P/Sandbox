namespace DrivenIt.Foundation.Contracts
{
    public interface IPrincipleContext
    {
        // some details about the current principle
        bool IsAnonymous { get; }

        string Name { get; }
    }
}
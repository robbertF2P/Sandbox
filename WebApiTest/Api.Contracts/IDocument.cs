using System;

namespace Api.Contracts
{
    public interface IDocument : IMetadata
    {
        Guid CompanyId { get; set; }
        string Name { get; set; }
        
        IComment[] Comments { get; set; }

        Guid Id { get; set; }
    }
}
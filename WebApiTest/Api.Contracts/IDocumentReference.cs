using System;

namespace Api.Contracts
{
    public interface IDocumentReference
    {
        string Name { get; set; }
        Guid Id { get; set; }
    }

}
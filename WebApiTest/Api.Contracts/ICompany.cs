using System;
using System.Collections.Generic;

namespace Api.Contracts
{
    public interface ICompany:IMetadata
    {
        string CompanyCode { get; set; }
        IEnumerable<IDocumentReference> Documents { get; set; }
        Guid Id { get; set; }
    }
}
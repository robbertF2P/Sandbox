using System;

namespace Api.Contracts
{
    public interface ICompanyReference
    {
        string CompanyCode { get; set; }
        Guid Id { get; set; }
    }

}
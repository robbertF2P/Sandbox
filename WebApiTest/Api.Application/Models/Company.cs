using System;
using System.Collections.Generic;
using Api.Application.Base;

namespace Api.Application.Models
{
    public class Company : BaseResource
    {
        public Company(Guid id)
        {
            Id = id;
        }

        public string CompanyCode { get; set; }

        public IEnumerable<DocumentReference> Documents { get; set; }
    }
}
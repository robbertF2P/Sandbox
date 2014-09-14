using System;
using System.Collections.Generic;
using Api.Dto.Models.Base;

namespace Api.Dto.Models
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
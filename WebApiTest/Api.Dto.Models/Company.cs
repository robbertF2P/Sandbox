using System.Collections.Generic;
using Api.Dto.Models.Base;
using Api.Dto.Models.Collections;

namespace Api.Dto.Models
{
    public class Company : BaseResource
    {
        public string CompanyCode { get; set; }

        public ResourceCollection<DocumentReference> Documents { get; set; }
    }
}
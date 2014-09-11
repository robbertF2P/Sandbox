using System;

namespace WebApiTest.Models
{
    public class CompanyReference
    {
        public Guid CompanyId { get; set; }

        public string CompanyCode { get; set; }

        //public Uri CompanyUri { get; set; }
    }
}
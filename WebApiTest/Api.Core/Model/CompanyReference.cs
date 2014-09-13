using System;
using Api.Core.Model.Base;

namespace Api.Core.Model
{
    public class CompanyReference:BaseResource
    {
        public static string RoutePrefix = "company";
        public CompanyReference() : base(RoutePrefix)
        {
        }
        
        public string CompanyCode { get; set; }
    }
}
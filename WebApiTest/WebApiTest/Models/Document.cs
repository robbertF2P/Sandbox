using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WebApiTest.Models
{
    #region "References"

    #endregion

    public class Document
    {
        public Document()
        {
            
        }

        public Guid Id { get; set; }

        public CompanyReference Company { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DocumentImportSource DocumentImportSource { get; set; }

    }

}
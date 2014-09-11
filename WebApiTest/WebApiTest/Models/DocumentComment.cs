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

    public class DocumentComment
    {
        public DocumentComment()
        {
            
        }

        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public UserReference CreatedBy { get; set; }

    }

}
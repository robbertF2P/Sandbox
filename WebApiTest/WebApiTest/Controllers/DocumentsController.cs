using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Http;
using WebApiTest.Models;

namespace WebApiTest.Controllers
{
    [RoutePrefix("api/documents")]
    public class DocumentsController : ApiController
    {
        [HttpGet]
        public IEnumerable<Document> Documents()
        {
            return new Document[] {};
        }

        [HttpGet]
        public Document Documents(int id)
        {
            return new Document();
        }

        [HttpDelete]
        [ActionName("documents")]
        public string DeleteDocument(int id, string reason)
        {
            return "Document was deleted due to reason " + reason;
        }

        [HttpPost]
        public string Import()
        {
            return "[ { Id: 123 } ]";
        }

        [HttpGet]
        [Route("{id}/comments")]
        public DocumentComment[] GetComments()
        {
            return new DocumentComment[]{};
        }

        [HttpPost]
        [Route("{id}/comments")]
        public void CreateComment(string comment)
        {
            return;
        }

        public class CreateTagRequest
        {
            public string Tag { get; set; }
            public string[] GeneralLedgers { get; set; }
            public string[] CostCenters { get; set; }
        }

        [HttpPost]
        [Route("{id}/tag")]
        public void CreateTag(CreateTagRequest req)
        {
            return;
        }

        //[HttpPost]
        //[Route("{id}/Tag")]
        //public void CreateTag(string tag, string[] generalLedgers, string[] costCenters, string[] authorizers)
        //{
        //    return;
        //}


    }
}
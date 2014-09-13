using System.Web.Http.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Api.Core.ModelFactory;
using Api.Dto.Models;
using Api.Dto.Models.Collections;

namespace Api.Core.Controllers
{
    [RoutePrefix(RoutePrefix)]
    public class DocumentsController:BaseApiController
    {
        public const string RoutePrefix = "documents";
        public const int DefaultPagesize = 10;

        public static IEnumerable<Document> Data = Factory.GetDummyData();

        [ResponseType(typeof(PagedResourceCollection<Document>))]
        public IHttpActionResult Get(string[] fields, int offSet=0, int limit=DefaultPagesize)
        {
            var items = Data.Skip(0).Take(limit);
            return Ok(ResourceCollectionFactory.CreateCollection(items,fields, Data.Count(),offSet,limit, GetCurrentRoot() + RoutePrefix));
        }
        
        [Route("{id:guid}")]
        [ResponseType(typeof(Document))]
        public IHttpActionResult Get(Guid id, string[] fields)
        {
            var document = Data.SingleOrDefault(d => d.Id == id);
            if (document == null) return base.NotFound();
            return Ok(ResourceFactory.CreateResource(document, fields, GetCurrentRoot() + RoutePrefix));
        }

        [Route("{id:guid}")]
        public IHttpActionResult Delete(Guid id)
        {
            Data = Data.Where(d => d.Id != id);
            return Ok();
        }

        [HttpPost]
        [Route("import")]
        public string Import(byte[] documentToImport)
        {
            return "[ { Id: 123 } ]";
        }

        [HttpGet]
        [Route("{id}/comments")]
        public DocumentComment[] GetComments()
        {
            return new DocumentComment[] { };
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
    }
}

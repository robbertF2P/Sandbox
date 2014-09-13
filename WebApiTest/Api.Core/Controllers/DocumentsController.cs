using System.Web.Http.Description;
using Api.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Api.Core.Model.Collections;

namespace Api.Core.Controllers
{
    [RoutePrefix("documents")]
    public class DocumentsController:ApiController
    {
        public static IEnumerable<Document> Data = Factory.GetDummyData();

        [ResponseType(typeof(DocumentCollection))]
        public IHttpActionResult Get([FromUri]string[] fields)
        {
            var items = Data;
            items.SetFields(fields);
            return Ok(new DocumentCollection(ToReference(items), 20));
        }
        
        [Route("{id:guid}")]
        [ResponseType(typeof(Document))]
        public IHttpActionResult Get(Guid id, string[] fields)
        {
            var document = Data.SingleOrDefault(d => d.Id == id);
            if (document == null) return base.NotFound();
            document.SetFields(fields);
            return Ok(document);
        }

        [Route("{id:guid}")]
        public IHttpActionResult Delete(Guid id)
        {
            Data = Data.Where(d => d.Id != id);
            return Ok();
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
        private IEnumerable<DocumentReference> ToReference(IEnumerable<Document> items)
        {
            return items.Select(document => new DocumentReference {Id = document.Id, Name = document.Name});
        }
    }
}

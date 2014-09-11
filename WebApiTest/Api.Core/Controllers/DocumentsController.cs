using Api.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Api.Core.Controllers
{
    [RoutePrefix("documents")]
    public class DocumentsController:ApiController
    {
        public static IEnumerable<Document> Data = Factory.GetDummyData(); 
       
        public IHttpActionResult Get(string[] fields)
        {
            var items = Data;
            items.SetFields(fields);
            return Ok(new DocumentCollection(ToReference(items), 20));
        }
        
        [Route("{id:guid}")]
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
        
        private IEnumerable<DocumentReference> ToReference(IEnumerable<Document> items)
        {
            return items.Select(document => new DocumentReference {Id = document.Id, Name = document.Name});
        }
    }
}

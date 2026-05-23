using Api.Application.Collections;
using Api.Application.Models;
using Api.Contracts.Collections;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api.Core.Controllers
{
    [RoutePrefix(RoutePrefix)]
    public class CompaniesController:BaseApiController
    {
        
        public const string RoutePrefix = "companies";
        public const int DefaultPagesize = 10;

        [ResponseType(typeof(PagedResourceCollection<Company>))]
        public IHttpActionResult Get(string[] fields, int offSet = 0, int limit = DefaultPagesize)
        {
            return Ok();
            //var items = Data.Skip(offSet).Take(limit);
            //return Ok(ResourceCollectionFactory.CreateCollection(items, fields, Data.Count(), offSet, limit, GetCurrentRoot() + RoutePrefix));
        }
    }
}

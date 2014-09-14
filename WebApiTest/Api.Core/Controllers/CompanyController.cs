using Api.Core.ModelFactory;
using Api.Dto.Models;
using Api.Dto.Models.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api.Core.Controllers
{
    [RoutePrefix(RoutePrefix)]
    public class CompaniesController:BaseApiController
    {
        private static IEnumerable<Company> Data = Factory.GetDummyCompanies();

        public const string RoutePrefix = "companies";
        public const int DefaultPagesize = 10;

        [ResponseType(typeof(PagedResourceCollection<Company>))]
        public IHttpActionResult Get(string[] fields, int offSet = 0, int limit = DefaultPagesize)
        {
            var items = Data.Skip(offSet).Take(limit);
            return Ok(ResourceCollectionFactory.CreateCollection(items, fields, Data.Count(), offSet, limit, GetCurrentRoot() + RoutePrefix));
        }
    }
}

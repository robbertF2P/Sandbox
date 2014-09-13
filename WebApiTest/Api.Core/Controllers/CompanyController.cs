using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Api.Core.ModelFactory;
using Api.Dto.Models;
using Api.Dto.Models.Collections;

namespace Api.Core.Controllers
{
    [RoutePrefix(RoutePrefix)]
    public class CompaniesController:BaseApiController
    {
        private static IEnumerable<Company> Data = new List<Company>
        {
            new Company
            {
                Id = Guid.NewGuid(),
                CompanyCode = "9901",
                Documents = new List<DocumentReference>
                {
                    new DocumentReference
                    {
                        Id = Guid.NewGuid(),
                        Name = "Document 1",
                    },
                    new DocumentReference
                    {
                        Id = Guid.NewGuid(),
                        Name = "Document 2",
                    }
                }
            }
        };
        public const string RoutePrefix = "companies";
        public const int DefaultPagesize = 10;

        [ResponseType(typeof(PagedResourceCollection<Company>))]
        public IHttpActionResult Get(string[] fields, int offSet = 0, int limit = DefaultPagesize)
        {
            var items = Data.Skip(0).Take(limit);
            return Ok(ResourceCollectionFactory.CreateCollection(items, fields, Data.Count(), offSet, limit, GetCurrentRoot() + RoutePrefix));
        }
    }
}

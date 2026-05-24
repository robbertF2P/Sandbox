using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Application.ModelFactory;
using Api.Contracts;
using Api.Contracts.Collections;

namespace Api.Application
{
    public class Provider : IDocumentsProvider
    {
        public const string RoutePrefix = "documents";
        public const int DefaultPagesize = 10;

        public IPagedResourceCollection<IDocument> GetDocuments(string[] fields, int offSet = 0, int limit = DefaultPagesize)
        {
            var data = Factory.GetDummyDocuments();
            var items = data.Skip(offSet).Take(limit);
            return ResourceCollectionFactory.CreateCollection(items, fields, data.Count(), offSet, limit, "http://localhost" + RoutePrefix);
        }
    }
}

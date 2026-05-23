using System;
using System.Collections.Generic;
using System.Linq;
using Api.Application.Base;
using Api.Application.Collections;
using Api.Contracts.Collections;

namespace Api.Application.ModelFactory
{
    public class ResourceCollectionFactory
    {
        public static IPagedResourceCollection<T> CreateCollection<T>(IEnumerable<T> items, string[] fields, int total, int currentOffset, int pageSize, string baseUrl, bool inspectProperties = true)
        {
            var lastOffset = Math.Max(total - pageSize, 0);
            var optionalFieldsItems = items as IEnumerable<IHaveOptionalFields>;
            if (optionalFieldsItems != null)
                    optionalFieldsItems.SetFields(fields);

            return new PagedResourceCollection<T>()
            {
                Items = items.Select(i => ResourceFactory.CreateResource(i,fields, baseUrl)),
                Total = total,
                _links = new Dictionary<string, Uri>( )
                {
                    {"self", new Uri(string.Format("{0}?offset={1}&limit={2}", baseUrl, currentOffset, pageSize))},
                    {"first", new Uri(string.Format("{0}?offset={1}&limit={2}", baseUrl, 0, pageSize))},

                    {"prev", new Uri(string.Format("{0}?offset={1}&limit={2}", baseUrl, Math.Max(currentOffset-pageSize,0), pageSize))},
                    {"next", new Uri(string.Format("{0}?offset={1}&limit={2}", baseUrl, Math.Min(currentOffset+pageSize, lastOffset), pageSize))},

                    {"last", new Uri(string.Format("{0}?offset={1}&limit={2}", baseUrl, lastOffset, pageSize))},
                }
            };
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Api.Dto.Models;
using Api.Dto.Models.Base;
using Api.Dto.Models.Collections;

namespace Api.Core.ModelFactory
{
    public class ResourceCollectionFactory
    {
        public static PagedResourceCollection<T> CreateCollection<T>(IEnumerable<T> items, string[] fields, int total, int currentOffset, int pageSize, string baseUrl, bool inspectProperties = true) where T:BaseResource
        {
            var lastOffset = Math.Max(total - pageSize, 0);
            var optionalFieldsItems = items as IEnumerable<IHaveOptionalFields>;
            if (optionalFieldsItems != null)
                    optionalFieldsItems.SetFields(fields);

            return new PagedResourceCollection<T>()
            {
                Items = items.Select(i => (T)ResourceFactory.CreateResource(i,fields, baseUrl)),
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

using System.Collections;
using System.Collections.Generic;
using Api.Dto.Models.Base;

namespace Api.Dto.Models.Collections
{
    public class ResourceCollection<T>:List<T> where T:BaseResource, IResourceCollection
    {
        public ResourceCollection(IEnumerable<T> items):base(items)
        {}
    }

    public interface IResourceCollection :IList
    {
    }
}
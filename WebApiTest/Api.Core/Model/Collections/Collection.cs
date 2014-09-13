using System.Collections.Generic;
using System.Linq;
using Api.Core.Model.Base;

namespace Api.Core.Model.Collections
{
    public abstract class Collection<T>:MetaLinks
    {
        protected Collection(IEnumerable<T> items, int total)
        {
            Items = items;
            Total = total;
            
            //AddLink("self", new Link());
            //AddLink("first", new Link());
            //AddLink("prev", new Link());
            //AddLink("next", new Link());
            //AddLink("last", new Link());
        }

        public int Total { get; private set; }
        public int Count { get { return Items.Count(); } }
        public IEnumerable<T> Items { get; private set; }
    }
}
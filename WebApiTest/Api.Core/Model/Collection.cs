using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Core.Model
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

    public class Link
    {
        public Link()
        {
            RoutePrefix = "dummy"; 
        }
        public string RoutePrefix { get; set; }
        public Guid Id { get; set; }

        public Uri ToUri()
        {
            return new Uri("Http://"+ RoutePrefix+"/" + Id);
        }
    }

    public abstract class MetaLinks
    {
        protected MetaLinks()
        {
            _links = new Dictionary<string, Uri>();
        }
        protected void AddLink(string rel, Link link)
        {
            _links.Add(rel, link.ToUri());
        }
        public IDictionary<string,Uri> _links { get; private set; }
    }

    public abstract class BaseResource:MetaLinks
    {
        public Guid Id { get; set; }

        protected BaseResource(string prefix)
        {
            AddLink("self", new Link { Id = Id, RoutePrefix = prefix});
        }
    }


    public class DocumentReference : BaseResource
    {
        public DocumentReference() : base(Document.RoutePrefix)
        {}

        public string Name { get; set; }
    }

    public class DocumentCollection : Collection<DocumentReference>
    {
        public DocumentCollection(IEnumerable<DocumentReference> items, int total) : base(items, total)
        {
        }

    }
}
using System;
using System.Collections.Generic;

namespace Api.Core.Model.Base
{
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
}
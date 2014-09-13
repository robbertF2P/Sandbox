using System;

namespace Api.Core.Model.Base
{
    public abstract class BaseResource:MetaLinks
    {
        public Guid Id { get; set; }

        protected BaseResource(string prefix)
        {
            AddLink("self", new Link { Id = Id, RoutePrefix = prefix});
        }
    }
}
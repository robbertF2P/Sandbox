using System;
using System.Collections.Generic;
using System.Linq;
using Api.Contracts.Collections;

namespace Api.Application.Collections
{
    public class PagedResourceCollection<T> : IPagedResourceCollection<T>
    {
        public int Total { get;  set; }
        public int Count { get
        {
            if (Items == null) return 0;
            return Items.Count(); } 
        }
        public IEnumerable<T> Items { get; set; }
        public IDictionary<string, Uri> _links { get; set; }
    }
}
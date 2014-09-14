using System;
using System.Collections.Generic;
using System.Linq;
using Api.Dto.Models.Base;

namespace Api.Dto.Models.Collections
{
    public class PagedResourceCollection<T> where T : BaseResource
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
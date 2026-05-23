using System;
using System.Collections.Generic;

namespace Api.Contracts.Collections
{
    public interface IPagedResourceCollection<T>
    {
        int Total { get; set; }
        int Count { get; }
        IEnumerable<T> Items { get; set; }
        IDictionary<string, Uri> _links { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Api.Contracts
{
    public interface IMetadata
    {
        IDictionary<string, Uri> _links { get; set; }
        IDictionary<string, Uri> _actions { get; set; }
    }
}
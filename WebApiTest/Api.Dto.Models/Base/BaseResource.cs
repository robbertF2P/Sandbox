using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Api.Dto.Models.Base
{
    public class BaseResource
    {
        public Guid Id { get; set; }
        public IDictionary<string, Uri> _links { get; set; }
        [JsonIgnore]
        public string[] OptionalFields { get; set; }
    }
}
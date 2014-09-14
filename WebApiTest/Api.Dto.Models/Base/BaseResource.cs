using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Api.Dto.Models.Base
{
    public class BaseResource
    {
        private Guid _id = Guid.Empty;
        
        public Guid Id
        {
            get { return _id; }
            set
            {
                if (_id != Guid.Empty || value == Guid.Empty) return;

                _id = value;
                _links = new Dictionary<string, Uri>()
                {
                    {"self",new Uri(UrlFactory.BaseUrl + UrlFactory.GetPrefix(this.GetType())+"/"+Id)}
                };
                _actions = new Dictionary<string, Uri>();
            }
        }

        public IDictionary<string, Uri> _links { get; set; }
        public IDictionary<string, Uri> _actions { get; set; }
        [JsonIgnore]
        public string[] OptionalFields { get; set; }
    }
}
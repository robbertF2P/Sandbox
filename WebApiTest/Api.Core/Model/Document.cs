using System;
using Api.Core.Attributes;
using Newtonsoft.Json;

namespace Api.Core.Model
{
    public class Document:BaseResource, IHaveOptionalFields
    {
        public Document():base(RoutePrefix)
        {
            Comments = new Comment[0];
        }

        public string Name { get; set; }
        [Optional]
        public Comment[] Comments { get; set; }
        [JsonIgnore]
        public string[] OptionalFields { get;  set; }

        public static string RoutePrefix { get { return "documents"; } }

    }
}
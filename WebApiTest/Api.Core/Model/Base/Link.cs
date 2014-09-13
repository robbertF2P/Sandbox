using System;

namespace Api.Core.Model.Base
{
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
}
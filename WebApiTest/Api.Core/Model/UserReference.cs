using System;

namespace Api.Core.Model
{
    public class UserReference
    {
        public Guid UserId { get; set; }
        public Uri UserUrl { get; set; }
    }
}
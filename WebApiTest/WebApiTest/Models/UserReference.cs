using System;

namespace WebApiTest.Models
{
    public class UserReference
    {
        public Guid UserId { get; set; }
        public Uri UserUrl { get; set; }
    }
}
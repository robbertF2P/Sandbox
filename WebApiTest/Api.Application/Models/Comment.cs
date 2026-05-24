using System;
using Api.Contracts;

namespace Api.Application.Models
{
   

    public class Comment : IComment
    {
        public string Text { get; set; }
        public DateTime CreatedOn { get; set; }
        public IUserReference CreatedBy { get; set; }
    }
}

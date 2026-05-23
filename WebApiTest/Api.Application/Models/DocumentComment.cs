using System;

namespace Api.Application.Models
{
    public class DocumentComment
    {

        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public UserReference CreatedBy { get; set; }
    }

}
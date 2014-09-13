using System;

namespace Api.Dto.Models
{
    public class DocumentComment
    {

        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public UserReference CreatedBy { get; set; }
    }

}
using System;

namespace Api.Core.Model
{
    #region "References"

    #endregion

    public class DocumentComment
    {
        public DocumentComment()
        {
            
        }

        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public UserReference CreatedBy { get; set; }

    }

}
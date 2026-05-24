using System;

namespace Api.Contracts
{
    public interface IComment
    {
        string Text { get; set; }
        DateTime CreatedOn { get; set; }
        IUserReference CreatedBy { get; set; }
    }
}
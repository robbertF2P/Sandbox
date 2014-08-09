using System;

namespace DrivenIt.Foundation.Infrastructure.Data
{
    public interface ISupportAudit
    {
        DateTime CreatedOn { get; set; }
        string CreatedBy { get; set; }
        DateTime ModifiedOn { get; set; }
        string ModifiedBy { get; set; }
    }
}
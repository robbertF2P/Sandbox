using System;
using System.Collections.Generic;
using ContextTests.Contracts.Model;

namespace ContextTests.Contracts
{
    public interface IAssignmentRepository : ISupportWorkSpace
    {
        IEnumerable<Assignment> GetAllAssignments();
        Guid Insert(Assignment assignment);
    }
}
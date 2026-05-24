using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ContextTests.Contracts.Model;

namespace ContextTests.Contracts
{
    public interface IStudentRepository
    {
        IEnumerable<Student> GetAllStudents();
        Guid Insert(Student student);
        void Persist();

        IQueryBuilder StartQuery();
        IEnumerable<Student> Find(IQueryBuilder query);
    }

    public interface IStudentRepositoryws : ISupportWorkSpace
    {
        IEnumerable<Student> GetAllStudents();
        Guid Insert(Student student);

    }
}

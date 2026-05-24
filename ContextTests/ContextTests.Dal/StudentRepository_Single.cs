using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ContextTests.Contracts;
using ContextTests.Contracts.Model;

namespace ContextTests.Dal
{
    public class StudentRepositorySingle: IStudentRepository
    {
        public IEnumerable<Student> GetAllStudents()
        {
            using (var cnt = new DataContext())
            {
                return AutoMapper.Mapper.Map<IEnumerable<Student>>(cnt.Student.ToList());
            }
        }

        public Guid Insert(Student student)
        {
            var newStudent = new Dal.Model.Student()
                {
                    Id = Guid.NewGuid(),
                    Name = student.Name
                };
            using (var cnt = new DataContext())
            {
                //Expression<Func<Model.Student, bool>> myExpression;
                //cnt.Student.Where(myExpression);
                cnt.Student.Add(newStudent);
                cnt.SaveChanges();
            }
            return newStudent.Id;
        }

        public void Persist()
        {
            // useless
        }

        public IQueryBuilder StartQuery()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Student> Find(IQueryBuilder query)
        {
            throw new NotImplementedException();
        }

        public Student Get(Guid id)
        {
            using (var cnt = new DataContext())
            {
                return AutoMapper.Mapper.Map<Student>(cnt.Student.Single(s=> s.Id == id));
            }
        }

        public void Set(IWorkSpace workSpace)
        {
            throw new NotImplementedException();
        }
    }
}

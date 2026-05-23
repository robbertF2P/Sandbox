using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ContextTests.Contracts;
using ContextTests.Contracts.Model;
using ContextTests.Dal.Query;
using LinqKit;

namespace ContextTests.Dal
{
    public class StudentRepositoryUow : IStudentRepository,IDisposable 
    {
        private readonly DataContext _context;

        public StudentRepositoryUow(DataContext context)
        {
            _context = context;
        }

        public IEnumerable<Student> GetAllStudents()
        {
            return AutoMapper.Mapper.Map<IEnumerable<Student>>(_context.Student.ToList());
        }

        public Guid Insert(Student student)
        {
            var newStudent = new Dal.Model.Student()
                {
                    Id = Guid.NewGuid(),
                    Name = student.Name,
                    EnrolledOn = DateTime.Now
                };
            _context.Student.Add(newStudent);
            return newStudent.Id;
        }

        public Student Get(Guid id)
        {
            return AutoMapper.Mapper.Map<Student>(_context.Student.Single(s => s.Id == id));
            
        }

        public void Persist()
        {
            _context.SaveChanges();
        }

        public IQueryBuilder StartQuery()
        {
            return new StudentQueryBuilder();
        }

        public IEnumerable<Student> Find(IQueryBuilder query)
        {
            var studentBuilder = query as StudentQueryBuilder;
            if (studentBuilder == null) throw new Exception("don't know that one");
            
            var pred = studentBuilder.Predicate;
            var students = _context.Student.AsExpandable().Where(pred.Compile());
            return AutoMapper.Mapper.Map<IList<Student>>(students);
        }

        #region dispose
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public void Set(IWorkSpace workSpace)
        {
            throw new NotImplementedException();
        }

        public IList<Student> DoExpressionTest()
        {
            DateTime yesterday = DateTime.Now.AddDays(-1);
            DateTime tomorrow = DateTime.Now.AddDays(1);
            Expression<Func<Model.Student, DateTime>> property = student => student.EnrolledOn;
            Func<DateTime, bool> datePredicate = date => date >= yesterday && date < tomorrow;
            Expression<Func<Model.Student, bool>> pred = student => datePredicate(property.Invoke(student));


            var entities = _context.Student.AsExpandable().Where(pred.Compile()).ToList();
            return AutoMapper.Mapper.Map<IList<Student>>(entities);
        }
    }
}
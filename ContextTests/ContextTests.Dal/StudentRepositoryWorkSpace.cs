using System;
using System.Collections.Generic;
using System.Linq;
using ContextTests.Contracts;
using ContextTests.Contracts.Model;

namespace ContextTests.Dal
{
    public class AssignmentRepositoryWorkSpace : IAssignmentRepository
    {
        private IWorkSpace _workSpace;
        private IQueryable<Model.Assignment> _assignments;

        public AssignmentRepositoryWorkSpace(IWorkSpace workSpace)
        {
            _workSpace = workSpace;
            _assignments = _workSpace.GetQueryable<Model.Assignment>();
        }
        public AssignmentRepositoryWorkSpace()
        { }
        public IEnumerable<Assignment> GetAllAssignments()
        {
            return AutoMapper.Mapper.Map<IEnumerable<Assignment>>(_assignments.ToList());
        }

        public Guid Insert(Assignment assignment)
        {
            var newAssignment = new Dal.Model.Assignment()
            {
                Id = Guid.NewGuid(),
                Description = assignment.Description
            };
            _workSpace.Add(newAssignment);
            return newAssignment.Id;
        }

        public void Set(IWorkSpace workSpace)
        {
            _workSpace = workSpace;
            _assignments = _workSpace.GetQueryable<Model.Assignment>();
        }
    }
    public class StudentRepositoryWorkSpace:IStudentRepositoryws
    {
        private IWorkSpace _workSpace;
        private IQueryable<Model.Student> _students;

        public StudentRepositoryWorkSpace(IWorkSpace workSpace)
        {
            _workSpace = workSpace;
            _students = _workSpace.GetQueryable<Model.Student>();
        }
        public StudentRepositoryWorkSpace()
        {}
        public IEnumerable<Student> GetAllStudents()
        {
            return AutoMapper.Mapper.Map<IEnumerable<Student>>(_students.ToList());
        }

        public Guid Insert(Student student)
        {
            var newStudent = new Dal.Model.Student()
                {
                    Id = Guid.NewGuid(),
                    Name = student.Name
                };
            _workSpace.Add(newStudent);
            return newStudent.Id;
        }
        
        public void Set(IWorkSpace workSpace)
        {
            _workSpace = workSpace;
            _students = _workSpace.GetQueryable<Model.Student>();
        }
    }
}
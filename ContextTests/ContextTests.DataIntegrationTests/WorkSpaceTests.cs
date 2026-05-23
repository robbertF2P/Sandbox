using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContextTests.Contracts;
using ContextTests.Contracts.Model;
using ContextTests.Dal;
using ContextTests.Locator;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContextTests.DataIntegrationTests
{
    [TestClass]
    public class WorkSpaceTests : BaseDateIntegrationTest
    {
        [TestMethod]
        public void Test()
        {
            Configure.Do();

            var ws = WorkSpaceFactory.Create();
            var _repo = new StudentRepositoryWorkSpace(ws);

            var newStudent = new Student { Name = "Robbert_aarg" };
            _repo.Insert(newStudent);


            _repo.GetAllStudents().Should().NotBeNull().And.HaveCount(0);

            ws.PersistAll();

            _repo.GetAllStudents().Should().NotBeNull().And.HaveCount(1);


        }

        [TestMethod]
        public void TestTwo()
        {
            Configure.Do();

            IStudentRepositoryws _repo = new StudentRepositoryWorkSpace();

            using (var ws = WorkSpaceFactory.Create(_repo))
            {
                var newStudent = new Student { Name = "Robbert_aarg" };
                _repo.Insert(newStudent);


                _repo.GetAllStudents().Should().NotBeNull().And.HaveCount(0);
                ws.PersistAll();

                _repo.GetAllStudents().Should().NotBeNull().And.HaveCount(1);
            }
        }
        [TestMethod]
        public void TestThree()
        {
            Configure.Do();

            IStudentRepositoryws _repoStudent = new StudentRepositoryWorkSpace();
            IAssignmentRepository _repoAssignment = new AssignmentRepositoryWorkSpace();

            using (var ws = WorkSpaceFactory.Create(_repoStudent,_repoAssignment))
            {
                var newStudent = new Student { Name = "Robbert_aarg" };
                _repoStudent.Insert(newStudent);
                _repoStudent.GetAllStudents().Should().NotBeNull().And.HaveCount(0);


                var assignment= new Assignment { Description = "Robbert_aarg" };
                _repoAssignment.Insert(assignment);
                _repoAssignment.GetAllAssignments().Should().NotBeNull().And.HaveCount(0);
                ws.PersistAll();

                _repoAssignment.GetAllAssignments().Should().NotBeNull().And.HaveCount(1);
                _repoStudent.GetAllStudents().Should().NotBeNull().And.HaveCount(1);
            }
        }
        [TestMethod]
        public void TestFour()
        {
            Configure.Do();

            IStudentRepositoryws _repoStudent = new StudentRepositoryWorkSpace();
            IAssignmentRepository _repoAssignment = new AssignmentRepositoryWorkSpace();

            using (var ws1 = WorkSpaceFactory.Create(_repoStudent))
            using (var ws2 = WorkSpaceFactory.Create(_repoAssignment))
            {
                var newStudent = new Student { Name = "Robbert_aarg" };
                _repoStudent.Insert(newStudent);
                _repoStudent.GetAllStudents().Should().NotBeNull().And.HaveCount(0);


                var assignment = new Assignment { Description = "Robbert_aarg" };
                _repoAssignment.Insert(assignment);
                _repoAssignment.GetAllAssignments().Should().NotBeNull().And.HaveCount(0);
                ws1.PersistAll();
                _repoAssignment.GetAllAssignments().Should().NotBeNull().And.HaveCount(0);
                _repoStudent.GetAllStudents().Should().NotBeNull().And.HaveCount(1);

                ws2.PersistAll();
                _repoAssignment.GetAllAssignments().Should().NotBeNull().And.HaveCount(1);
                _repoStudent.GetAllStudents().Should().NotBeNull().And.HaveCount(1);

            }
        }
    }

    public class WorkSpaceFactory
    {


        public static IWorkSpace Create(params ISupportWorkSpace[] workSpaceUsers)
        {
            var result = new DbWorkSpace();
            foreach (var wsu in workSpaceUsers)
            {
                wsu.Set(result);
            }
            return result;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class StudentRepositoryTest: BaseDateIntegrationTest
    {
        private IStudentRepository _repo;

        [TestInitialize]
        public void Init()
        {
            Configure.Do();
            _repo = new StudentRepositorySingle();

        }
        [TestMethod]
        public void GetAll()
        {
            var result = _repo.GetAllStudents();
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void InsertStudent()
        {
            var newStudent = new Student {Name = "Robbert_aarg"};
            _repo.Insert(newStudent);

            var result = _repo.GetAllStudents();
            result.Should().NotBeNull().And.HaveCount(1);
        }

        [TestMethod]
        public void Insert300Students()
        {
            var stopwatch = new Stopwatch();

            var repo = new StudentRepositorySingle();
            stopwatch.Start();
            for (int i = 0; i < 300; i++)
            {
                var newStudent = new Student {Name = "Robbert" +i};
                repo.Insert(newStudent);
            }
            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0}",stopwatch.Elapsed);

            var result = repo.GetAllStudents();
            result.Should().NotBeNull().And.HaveCount(300);
        }

        [TestMethod]
        public void Read300Students()
        {
            var ids = new List<Guid>();
            var repo = new StudentRepositorySingle();
            for (int i = 0; i < 300; i++)
            {
                var newStudent = new Student { Name = "Robbert" + i };
                ids.Add(repo.Insert(newStudent));
            }
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            for (int i = 0; i < 300; i++)
            {
                var student = repo.Get(ids[i]);
            }
            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        }
        [TestMethod]
        public void Insert300StudentsUow()
        {
            var stopwatch = new Stopwatch();
            var context = new DataContext();
            var repo = new StudentRepositoryUow(context);
            stopwatch.Start();
            for (int i = 0; i < 300; i++)
            {
                var newStudent = new Student { Name = "Robbert" + i };
                repo.Insert(newStudent);
            }
            repo.Persist();
            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);

            var result = repo.GetAllStudents();
            result.Should().NotBeNull().And.HaveCount(300);
        }

        [TestMethod]
        public void Read300StudentsUow()
        {
            var ids = new List<Guid>();
            var context = new DataContext();
            var repo = new StudentRepositoryUow(context);
            for (int i = 0; i < 300; i++)
            {
                var newStudent = new Student { Name = "Robbert" + i };
                ids.Add(repo.Insert(newStudent));
            }
            repo.Persist();
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            for (int i = 0; i < 300; i++)
            {
                var student = repo.Get(ids[i]);
            }
            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        }

        [TestMethod]
        public void MyTest()
        {
            var context = new DataContext();
            var repo = new StudentRepositoryUow(context);
            var newStudent = new Student { Name = "Robbert"};
            repo.Insert(newStudent);
            repo.Persist();

            //var students = repo.DoExpressionTest();
            var query = repo.StartQuery();
            query.Field<DateTime>("enrolledon").IsBetween(DateTime.Now.AddDays(-1), DateTime.Today.AddDays(1));
            var students = repo.Find(query);
            students.Should().NotBeNull();
            students.Should().HaveCount(1);
        }
    }
}

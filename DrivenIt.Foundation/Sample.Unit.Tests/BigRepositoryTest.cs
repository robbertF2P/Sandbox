using System.Data.Entity;
using AutoMapper;
using DrivenIt.Foundation.Contracts;
using DrivenIt.Foundation.Data.TestTools;
using DrivenIt.Foundation.Infrastructure.Data;
using DrivenIt.Foundation.Sample.Contracts;
using DrivenIt.Foundation.Sample.DataLayer;
using DrivenIt.Foundation.Sample.DataLayer.Automapper;
using DrivenIt.Foundation.Sample.DataLayer.Repository;
using NUnit.Framework;

namespace Sample.Unit.Tests
{
    public class DBCrator : ICreateDataContext
    {
        public DbContext Create()
        {
            return new DataContext();
        }
    }
    [TestFixture]
    public class BigRepositoryTest : BaseDataIntegrationTest
    {
        static BigRepositoryTest()
        {
            BaseDataIntegrationTest.DatabaseManagerFactory = () => new DatabaseMigrator(new DBCrator(),  "10.168.0.2", "sa", "treffer_05");
            
            DataContextUow.ContextFactory = () => new DataContext();
            Mapper.AddProfile<DataProfile>();
        }

        [Test]
        public void TestCreate()
        {
            
            var repo = new TheBigRepository(new DataContextUow(new Principle()));
            using (var uow = new UnitOfWorkFactory(new Principle()).StartUnitOfWork(repo))
            {
                repo.CreateNew(new CreateTheThing() {TheNameToUse = "myname"});
                uow.SaveChanges();
            }
        }
    }

    public class Principle : IPrincipleContext
    {
        public bool IsAnonymous { get; private set; }
        public string Name { get; private set; }
    }
}
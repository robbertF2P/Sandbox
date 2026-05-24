using DrivenIt.Foundation.Contracts;
using NUnit.Framework;
using System;

namespace DrivenIt.Foundation.Data.TestTools
{
    [TestFixture]
    public abstract class BaseDataIntegrationTest
    {
        public static Func<IManageDatabase> DatabaseManagerFactory;
        private static TestDatabaseManager Manager;

        [TestFixtureSetUp]
        public void AssemblyInit()
        {
            //new Data.Bootstrap.Mappings().Execute();
            Manager = new TestDatabaseManager(DatabaseManagerFactory());
            Manager.SetupDatabase();
        }

        [TestFixtureTearDown]
        public void AssemblyCleanup()
        {
            Manager.RemoveDatabase();
        }
        [SetUp]
        public void TestInit()
        {
            Manager.BeforeTest();
        }

        [TearDown]
        public void TestClean()
        {
            Manager.AfterTest();
        }
    }
}

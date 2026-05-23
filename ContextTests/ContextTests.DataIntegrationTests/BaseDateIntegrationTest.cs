using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContextTests.DataIntegrationTests
{
    [TestClass]
    public abstract class BaseDateIntegrationTest
    {
        private static readonly TestDatabaseManager _manager = new TestDatabaseManager();
        
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            _manager.SetupDatabase();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            _manager.RemoveDatabase();
        }

        [TestInitialize]
        public void TestInit()
        {
            _manager.BeforeTest();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _manager.AfterTest();
        }
    }
}

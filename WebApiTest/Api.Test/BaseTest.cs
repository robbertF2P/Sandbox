using Api.Core;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Api.Test
{
    public abstract class BaseTest
    {
        protected TestServer _server;

        [TestFixtureSetUp]
        public void FixtureInit()
        {
            _server = TestServer.Create<Startup>();
        }

        [TestFixtureTearDown]
        public void FixtureDispose()
        {
            _server.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Owin.WebApi.Test
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public  async void TestSample()
        {
            using(var server = TestServer.Create(app =>
                {
                    app.UseErrorPage(); // See Microsoft.Owin.Diagnostics
                    app.Run(context =>
                    {
                        return context.Response.WriteAsync("Hello world using OWIN TestServer");
                    });
                }))
            {
                var response = await server.HttpClient.GetAsync("/");
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Be("Hello world using OWIN TestServer");
            }}
        
    }

    [TestFixture]
    public class TestApi
    {
        private TestServer _server;

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
        [Test]
        public async void WebApiGetAllTest()
        {
            var response = await _server.HttpClient.GetAsync("/api/Test/Do?fields=hoi,dag,fiets");
            var result = await response.Content.ReadAsStringAsync();
            
            result.Should().NotBeEmpty();
            //Assert.AreEqual(2, result.Count());
            //Assert.AreEqual("hello", result.First());
            //Assert.AreEqual("world", result.Last());
        }
        [Test]
        public async void WebApiGetAllTest_array()
        {
            var response = await _server.HttpClient.GetAsync("/api/Test/Done?fields=12,4,6");
            var result = await response.Content.ReadAsStringAsync();

            result.Should().NotBeEmpty();
            //Assert.AreEqual(2, result.Count());
            //Assert.AreEqual("hello", result.First());
            //Assert.AreEqual("world", result.Last());
        }
    }
}

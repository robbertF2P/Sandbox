using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api.Core;
using Api.Core.Model;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Api.Test
{
    [TestFixture]
    public class DocumentsApiTest
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
        public async void Test_get_collection()
        {
            var response = await _server.HttpClient.GetAsync("/documents");
            var content = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue();
            Console.Write(content);
            var collection = JsonConvert.DeserializeObject<DocumentCollection>(content);
            collection.Items.Should().NotBeEmpty();
        }


        [Test]
        public async void Test_get_single()
        {
            var documents = await Documents();
            var response = await _server.HttpClient.GetAsync("/documents/"+documents.First().Id);
            var content = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue();
            Console.Write(content);
            var document = JsonConvert.DeserializeObject<Document>(content);

            document.Should().NotBeNull();
            document.Comments.Should().BeEmpty();
        }

        [Test]
        public async void Test_get_single_with_OptionalFields()
        {
            var documents = await Documents();
            var response = await _server.HttpClient.GetAsync(string.Format("/documents/{0}?fields=comments", documents.First().Id));
            var content = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue();
            Console.Write(content);
            var document = JsonConvert.DeserializeObject<Document>(content);

            document.Should().NotBeNull();
            document.Comments.Should().HaveCount(1);
        }

        [Test]
        public async void Test_delete()
        {
            var response = await _server.HttpClient.DeleteAsync("/documents/D235E56A-3994-4954-BC4B-E443AF68EC00");
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        private async Task<IEnumerable<DocumentReference>> Documents()
        {
            var response = await _server.HttpClient.GetAsync("/documents");
            var content = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue();
            var collection = JsonConvert.DeserializeObject<DocumentCollection>(content);
            return collection.Items;
        }
    }
}

using Api.Dto.Models;
using Api.Dto.Models.Collections;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Test
{
    [TestFixture]
    public class DocumentsApiTest:BaseTest
    {
        [Test]
        public async void Test_get_collection()
        {
            var response = await _server.HttpClient.GetAsync("/documents");
            var content = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue();
            Console.Write(content);
            var collection = JsonConvert.DeserializeObject<PagedResourceCollection<Document>>(content);
            collection.Items.Should().NotBeEmpty();
        }

        [Test]
        public async void Test_get_collection_with_paging()
        {
            var response = await _server.HttpClient.GetAsync("/documents?offset=2&limit=1");
            var content = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue();
            Console.Write(content);
            var collection = JsonConvert.DeserializeObject<PagedResourceCollection<Document>>(content);
            collection.Items.Should().NotBeEmpty();
            collection.Items.Should().HaveCount(1);
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
            document.Comments.Should().BeNullOrEmpty();
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

        private async Task<IEnumerable<Document>> Documents()
        {
            var response = await _server.HttpClient.GetAsync("/documents");
            var content = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue();
            var collection = JsonConvert.DeserializeObject<PagedResourceCollection<Document>>(content);
            return collection.Items;
        }
    }
}

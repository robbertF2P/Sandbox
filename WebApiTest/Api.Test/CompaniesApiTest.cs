using System;
using Api.Dto.Models;
using Api.Dto.Models.Collections;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Api.Test
{
    [TestFixture]
    public class CompaniesApiTest : BaseTest
    {
        [Test]
        public async void Test_get_collection()
        {
            var response = await _server.HttpClient.GetAsync("/companies");
            var content = await response.Content.ReadAsStringAsync();

            response.IsSuccessStatusCode.Should().BeTrue();
            Console.Write(content);
            var collection = JsonConvert.DeserializeObject<PagedResourceCollection<Company>>(content);
            collection.Items.Should().NotBeEmpty();
        }
    }
}
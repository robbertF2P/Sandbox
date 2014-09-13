using System;
using System.Collections.Generic;
using Api.Dto.Models;

namespace Api.Core.Controllers
{
    public class Factory
    {
        public static IEnumerable<Document> GetDummyData()
        {
            return new[]
            {
                new Document
                {
                    Id = Guid.NewGuid(),
                    Name = "Document1",
                    Comments = new Comment[]
                    {
                        new Comment() {CreatedOn = DateTime.Now, Text = "great stuff"}
                    }
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    Name = "Document2",
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    Name = "Document3"
                },
            };
        }
    }
}
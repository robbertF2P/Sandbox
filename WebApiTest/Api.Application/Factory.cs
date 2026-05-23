using System;
using System.Collections.Generic;
using Api.Application.Models;
using Api.Contracts;

namespace Api.Application
{
    public class Factory
    {
        public static IEnumerable<IDocument> GetDummyDocuments()
        {
            return new[]
            {
                Document.CreateDocument(Guid.NewGuid(),Guid.NewGuid(),"Document 1",new []
                    {
                        new Comment() {CreatedOn = DateTime.Now, Text = "great stuff"}
                    }),
                Document.CreateDocument(Guid.NewGuid(),Guid.NewGuid(),"Document 2"),
                Document.CreateDocument(Guid.NewGuid(),Guid.NewGuid(),"Document 3"),
            };
        }

        public static IEnumerable<Company> GetDummyCompanies()
        {
            return new List<Company>
            {
                new Company(Guid.NewGuid())
                {
                    CompanyCode = "9901",
                    Documents = new List<DocumentReference>
                    {
                        new DocumentReference()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Document 1",
                        },
                        new DocumentReference()
                        {
                            Id = Guid.NewGuid(),
                            Name = "Document 2",
                        }
                    }
                }
            };
        }
    }
}
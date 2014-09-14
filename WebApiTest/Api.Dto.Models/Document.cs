using System;
using System.Diagnostics;
using Api.Dto.Models.Attributes;
using Api.Dto.Models.Base;

namespace Api.Dto.Models
{
    public class Document : BaseResource, IHaveOptionalFields
    {
        public static Document CreateDocument(Guid id, Guid companyId, string name, Comment[] comments = null)
        {
            var result = new Document(id)
            {
                CompanyId = companyId,
                Name = name,
                Comments = comments
            };
            result._actions["import"] = new Uri(UrlFactory.BaseUrl + UrlFactory.GetPrefix(typeof(Document)) + "/import?companyId=" + companyId);
            return result;
        }

        public Document(Guid id)
        {
            Id = id;
        }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        [Optional]
        public Comment[] Comments { get; set; }
    }
}
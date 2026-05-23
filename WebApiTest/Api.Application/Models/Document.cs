using System;
using Api.Application.Base;
using Api.Contracts;

namespace Api.Application.Models
{
    public class Document : BaseResource, IHaveOptionalFields, IDocument
    {
        public static Document CreateDocument(Guid id, Guid companyId, string name, IComment[] comments = null)
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
        public IComment[] Comments { get; set; }
    }
}

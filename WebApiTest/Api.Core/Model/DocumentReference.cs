using Api.Core.Model.Base;

namespace Api.Core.Model
{
    public class DocumentReference : BaseResource
    {
        public DocumentReference() : base(Document.RoutePrefix)
        {}

        public string Name { get; set; }
    }
}
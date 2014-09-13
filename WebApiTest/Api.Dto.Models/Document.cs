using Api.Dto.Models.Attributes;
using Api.Dto.Models.Base;

namespace Api.Dto.Models
{
    public class Document : BaseResource, IHaveOptionalFields
    {
        public string Name { get; set; }
        [Optional]
        public Comment[] Comments { get; set; }
    }
}
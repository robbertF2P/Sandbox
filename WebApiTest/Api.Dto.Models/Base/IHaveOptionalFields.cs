namespace Api.Dto.Models.Base
{
    public interface IHaveOptionalFields
    {
        string[] OptionalFields { get; set; }
    }
}
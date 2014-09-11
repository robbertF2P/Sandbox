using System.Collections.Generic;

namespace Api.Core.Model
{
    public interface IHaveOptionalFields
    {
        string[] OptionalFields { get; set; }
    }

    public static class Utils
    {

        public static void SetFields(this IHaveOptionalFields item, string[] fields)
        {
            item.OptionalFields = fields;
        }
        public static void SetFields(this IEnumerable<IHaveOptionalFields> items, string[] fields)
        {
            foreach (var item in items)
            {
                item.OptionalFields = fields;
            }
        }
    }
}
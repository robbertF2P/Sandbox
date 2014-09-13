using System.Collections.Generic;
using Api.Dto.Models.Base;

namespace Api.Dto.Models
{
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
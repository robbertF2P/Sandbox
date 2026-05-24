using Api.Application.Base;

namespace Api.Application.ModelFactory
{
    public class ResourceFactory
    {
        public static T CreateResource<T>(T item,string[] fields, string baseUrl) 
        {
            var optionalFieldsItem = item as IHaveOptionalFields;
            if (optionalFieldsItem != null) 
                optionalFieldsItem.SetFields(fields);

            //item._links = new Dictionary<string, Uri>
            //{
            //    {"self", new Uri(string.Format("{0}/{1}", baseUrl, item.Id))},
            //};
            
            return item;
        }
    }
}
using System;
using System.Collections.Generic;
using Api.Dto.Models;
using Api.Dto.Models.Base;

namespace Api.Core.ModelFactory
{
    public class ResourceFactory
    {
        public static BaseResource CreateResource<T>(T item,string[] fields, string baseUrl) where T:BaseResource
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
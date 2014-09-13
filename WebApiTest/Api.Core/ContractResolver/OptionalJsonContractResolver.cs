using Api.Dto.Models;
using Api.Dto.Models.Attributes;
using Api.Dto.Models.Base;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;

namespace Api.Core.ContractResolver
{

    public class OptionalJsonContractResolver : CamelCasePropertyNamesContractResolver
    {
        
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);
            if (!objectType.GetInterfaces().Contains(typeof(IHaveOptionalFields))) return contract;
            foreach (var property in contract.Properties.Where(p => !p.Ignored))
            {
                var propertyInfo = objectType.GetProperties().SingleOrDefault(p => 
                    String.Equals(p.Name, property.PropertyName, StringComparison.CurrentCultureIgnoreCase)
                    && p.PropertyType == property.PropertyType);
                if (propertyInfo != null && propertyInfo.GetCustomAttributes(typeof(OptionalAttribute), false).Any())
                    property.ShouldSerialize = (o) =>
                    {
                        var dto = (o as IHaveOptionalFields);
                        return dto != null 
                            && dto.OptionalFields!= null 
                            && dto.OptionalFields.Any(f=> String.Equals(f, property.PropertyName, StringComparison.CurrentCultureIgnoreCase));
                    };
                else
                    property.ShouldSerialize = (o) => true;
            }

            return contract;
        }
    }
}

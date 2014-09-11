using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Api.Core.ActionFilter
{
    public class ArrayInputAttribute : ActionFilterAttribute
    {
        public char Separator { get; set; }
        private readonly string _parameterName;

        public ArrayInputAttribute(string parameterName)
        {
            _parameterName = parameterName;
            Separator = ',';
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ActionArguments.ContainsKey(_parameterName)) return;

            var parameters = string.Empty;
            var routeValues = actionContext.ControllerContext.RouteData.Values;
            if (routeValues.ContainsKey(_parameterName))
            {
                parameters = (string) routeValues[_parameterName];
            }
            else
            {
                var requestValues = actionContext.ControllerContext.Request.RequestUri.ParseQueryString();
                if (requestValues[_parameterName] != null)
                {
                    parameters = requestValues[_parameterName];
                }
            }

            var arrayAsString = parameters.Split(Separator);
            actionContext.ActionArguments[_parameterName] = arrayAsString.ToArray();
        }

    }
}

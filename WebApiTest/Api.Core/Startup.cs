using Api.Core.ActionFilter;
using Api.Core.ContractResolver;
using Owin;
using System.Web.Http;

namespace Api.Core
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureWebApi(app);
        }

        private static void ConfigureWebApi(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            
            config.Filters.Add(new ArrayInputAttribute("fields"));

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new OptionalJsonContractResolver();
            config.Formatters.JsonFormatter.Indent = true;

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}"
            );

            app.UseWebApi(config);
        }
    }
}

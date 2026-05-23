using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebParts.Startup))]
namespace WebParts
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}

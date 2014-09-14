using System;
using System.Collections.Generic;
using System.Linq;
using Api.Core;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(IIS.Host.Startup))]

namespace IIS.Host
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<LoggerModule>();
            ConfigureAuth(app);
            
        }
    }
}

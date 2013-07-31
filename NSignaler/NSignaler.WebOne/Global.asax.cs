using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NServiceBus;
using NSignaler.Web.Shared;
using SignalR.Castle.Windsor;

namespace NSignaler.WebOne
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static IWindsorContainer _container;
        public static IBus Bus { get; set; }

        protected void Application_Start()
        {
            
            AreaRegistration.RegisterAllAreas();

            Bus = Configure.With()
							.DefineEndpointName("NSignaler.WebOne")
                            .DefaultBuilder()
							.JsonSerializer()
							.MsmqTransport()
                            .UnicastBus()
                            .CreateBus()
                            .Start(() => Configure.Instance.ForInstallationOn<NServiceBus.Installation.Environments.Windows>().Install());
            BootstrapContainer();
            //GlobalHost.DependencyResolver.Register(typeof (IBus),()=> Bus);
            // Register the default hubs route: ~/signalr
            RouteTable.Routes.MapHubs();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static void BootstrapContainer()
        {
            _container = new WindsorContainer().Install(FromAssembly.InThisApplication());
            _container.Register(Component.For<IBus>().Instance(Bus));
            GlobalHost.DependencyResolver = new WindsorDependencyResolver(_container);
            var controllerFactory = new WindsorControllerFactory(_container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
        }
        protected void Application_End()
        {
            _container.Dispose();
        }
    }
    public class ControllersInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromThisAssembly()
                                .BasedOn<IController>()
                                .LifestyleTransient());
        }
    }
}

namespace NSignaler
{
    public class HubsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory+@"bin\";
            container.Register(Classes.FromAssemblyNamed("NSignaler.Web.Shared")//.FromAssemblyInDirectory(new AssemblyFilter(path))
                                      .BasedOn<IHub>()
                                      .LifestyleTransient());
        }
    }
}
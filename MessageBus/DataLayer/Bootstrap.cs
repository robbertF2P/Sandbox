using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace DataLayer
{
    public class Bootstrap : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromThisAssembly()
                                      .InNamespace("DataLayer.Repositories")
                                      .WithService.AllInterfaces()
                                      .LifestyleTransient());
        }

    }
}
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Application.Layer
{
    public class Bootstrap : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromThisAssembly()
                                      .InNamespace("Application.Layer")
                                      .WithService.AllInterfaces()
                                      .LifestyleTransient());
        }

    }
}
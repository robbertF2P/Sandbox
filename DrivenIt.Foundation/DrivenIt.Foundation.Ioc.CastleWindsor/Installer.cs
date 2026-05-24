using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DrivenIt.Foundation.Contracts.UnitOfWork;
using DrivenIt.Foundation.Infrastructure.Data;

namespace DrivenIt.Foundation.Ioc.CastleWindsor
{
    public class Installer:IComponentsInstaller
    {
        public void SetUp(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                .For<IUow>()
                .ImplementedBy<DataContextUow>(),
                Component.
                For<IUowFactory>()
                .ImplementedBy<UnitOfWorkFactory>());
        }
    }
}

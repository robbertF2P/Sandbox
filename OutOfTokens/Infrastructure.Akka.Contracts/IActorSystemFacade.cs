using Akka.Actor;
using System.Threading.Tasks;

namespace Infrastructure.Akka.Contracts
{
    public interface IActorSystemFacade
    {
        void Tell<TCommand>(TCommand command) where TCommand : IActorCommand;

        Task<IActorRef> RegisterActor(string name, Props props);
    }
}

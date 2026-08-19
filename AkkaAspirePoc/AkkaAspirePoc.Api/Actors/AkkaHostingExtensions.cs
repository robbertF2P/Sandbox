using Aaron.Akka.Aspire;
using Aaron.Akka.Discovery.Redis;
using Akka.Actor;
using Akka.Hosting;
using AkkaAspirePoc.Api.Actors.Messages;

namespace AkkaAspirePoc.Api.Actors;

public sealed class TodoActorFacade(IActorRef todoActor)
{
    private static readonly TimeSpan AskTimeout = TimeSpan.FromSeconds(10);

    public Task<CreateTodoResult> CreateAsync(string title, CancellationToken cancellationToken = default) =>
        todoActor.Ask<CreateTodoResult>(new CreateTodoCommand(title), AskTimeout, cancellationToken);

    public Task<GetTodosResult> GetAllAsync(CancellationToken cancellationToken = default) =>
        todoActor.Ask<GetTodosResult>(new GetTodosQuery(), AskTimeout, cancellationToken);

    public Task<CompleteTodoResult> CompleteAsync(Guid todoId, CancellationToken cancellationToken = default) =>
        todoActor.Ask<CompleteTodoResult>(new CompleteTodoCommand(todoId), AskTimeout, cancellationToken);
}

public static class AkkaHostingExtensions
{
    public static IServiceCollection AddTodoActors(this IServiceCollection services)
    {
        services.AddAkka("TodoCluster", (akkaBuilder, sp) =>
        {
            akkaBuilder.WithAspireClusterBootstrap(sp,
                configureDiscovery: (b, config) =>
                {
                    var redisConn = config.GetConnectionString("akka-discovery");
                    if (!string.IsNullOrEmpty(redisConn))
                    {
                        b.WithRedisDiscovery(redisConn, config["Akka:Cluster:ServiceName"]);
                    }
                },
                clusterConfigure: c => c.Roles = ["todo-api"]);

            akkaBuilder.WithActors((system, registry, resolver) =>
            {
                var props = resolver.Props<TodoActor>();
                var actor = system.ActorOf(props, "todo-actor");
                registry.Register<TodoActor>(actor);
            });
        });

        services.AddSingleton<TodoActorFacade>(sp =>
        {
            var registry = sp.GetRequiredService<IRequiredActor<TodoActor>>();
            return new TodoActorFacade(registry.ActorRef);
        });

        return services;
    }
}

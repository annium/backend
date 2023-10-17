using System.Linq;
using Annium.Mesh.Server;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal;
using Annium.Mesh.Server.Internal.Handlers;
using Annium.Mesh.Server.Internal.Handlers.Subscriptions;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshServer(this IServiceContainer container)
    {
        // public
        container.Add<ICoordinator, Coordinator>().Singleton();

        // internal
        container.Add<ServerLifetime>().AsInterfaces().Singleton();
        container.Add<ConnectionTracker>().AsSelf().Singleton();
        container.Add<BroadcastCoordinator>().AsSelf().Singleton();
        container.Add<ConnectionHandlerFactory>().AsSelf().Singleton();
        container.Add<LifeCycleCoordinator>().AsSelf().Scoped();
        container.Add<MessageHandler>().AsSelf().Transient();
        container.Add<PusherCoordinator>().AsSelf().Singleton();

        // internal - handlers
        container.Add<SubscriptionContextStore>().AsSelf().AsInterfaces().Singleton();
        container.AddAll()
            .AssignableTo<LifeCycleHandlerBase>()
            .As<LifeCycleHandlerBase>()
            .Scoped();

        // handlers
        container.AddBroadcasters();
        container.AddPushers();

        return container;
    }

    private static void AddBroadcasters(this IServiceContainer container)
    {
        var types = container.GetTypeManager().GetImplementations(typeof(IBroadcaster<>));
        foreach (var type in types)
        {
            var messageType = type.GetInterfaces()
                .Single(x => x.Name == typeof(IBroadcaster<>).Name)
                .GetGenericArguments()[0];
            container.Add(type).AsInterfaces().Singleton();
            container.Add(typeof(BroadcasterRunner<>).MakeGenericType(messageType))
                .As(typeof(IBroadcasterRunner))
                .Singleton();
        }
    }

    private static void AddPushers(this IServiceContainer container)
    {
        var types = container.GetTypeManager().GetImplementations(typeof(IPusher<>));
        foreach (var type in types)
        {
            var messageType = type.GetInterfaces()
                .Single(x => x.Name == typeof(IPusher<>).Name)
                .GetGenericArguments()[0];
            container.Add(type).AsInterfaces().Singleton();
            container.Add(typeof(PusherRunner<>).MakeGenericType(messageType))
                .As(typeof(IPusherRunner))
                .Singleton();
        }
    }
}
using System;
using Annium.Mesh.Server;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal;
using Annium.Mesh.Server.Internal.Models;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshServer(
        this IServiceContainer container,
        Action<ServerConfigurationOptions> configure
    )
    {
        // public - base
        var options = new ServerConfigurationOptions(container);
        configure(options);
        container.Add(options).AsSelf().Singleton();
        container.Add<ICoordinator, Coordinator>().Singleton();

        // internal - server level components
        container.Add<ServerLifetime>().AsInterfaces().Singleton();
        container.Add<ConnectionTracker>().AsSelf().Singleton();
        // container.Add<BroadcastCoordinator>().AsSelf().Singleton();

        // internal - server level stores
        // container.Add<SubscriptionContextStore>().AsSelf().AsInterfaces().Singleton();

        // internal - connection level components
        container.Add<ConnectionHandler>().AsSelf().Scoped();
        container.Add<ConnectionContext>().AsSelf().Scoped();
        container.Add<LifeCycleCoordinator>().AsSelf().Scoped();
        container.Add<MessageHandler>().AsSelf().Scoped();
        container.Add<PushCoordinator>().AsSelf().Scoped();

        // internal - handlers
        container.AddAll()
            .AssignableTo<LifeCycleHandlerBase>()
            .As<LifeCycleHandlerBase>()
            .Scoped();

        return container;
    }

    // private static void AddBroadcasters(this IServiceContainer container)
    // {
    //     var types = container.GetTypeManager().GetImplementations(typeof(IBroadcaster<>));
    //     foreach (var type in types)
    //     {
    //         var messageType = type.GetInterfaces()
    //             .Single(x => x.Name == typeof(IBroadcaster<>).Name)
    //             .GetGenericArguments()[0];
    //         container.Add(type).AsInterfaces().Singleton();
    //         container.Add(typeof(BroadcasterRunner<>).MakeGenericType(messageType))
    //             .As(typeof(IBroadcasterRunner))
    //             .Singleton();
    //     }
    // }
    //
    // private static void AddPushers(this IServiceContainer container)
    // {
    //     var types = container.GetTypeManager().GetImplementations(typeof(IPusher<>));
    //     foreach (var type in types)
    //     {
    //         var messageType = type.GetInterfaces()
    //             .Single(x => x.Name == typeof(IPusher<>).Name)
    //             .GetGenericArguments()[0];
    //         container.Add(type).AsInterfaces().Singleton();
    //         container.Add(typeof(PusherRunner<>).MakeGenericType(messageType))
    //             .As(typeof(IPusherRunner))
    //             .Singleton();
    //     }
    // }
}
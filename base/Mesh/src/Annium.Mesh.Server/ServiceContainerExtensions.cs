using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Mesh.Server;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Internal;
using Annium.Mesh.Server.Internal.Handlers;
using Annium.Mesh.Server.Internal.Handlers.Subscriptions;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Server.Internal.Serialization;
using Annium.Mesh.Server.Models;
using Annium.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddWebSocketServer<TState>(
        this IServiceContainer container,
        Action<IServiceProvider, ServerConfiguration> configure
    )
        where TState : ConnectionStateBase
    {
        // public
        container.Add<ICoordinator, Coordinator<TState>>().Singleton();
        container.Add<Func<Guid, TState>>(sp => cnId =>
        {
            var state = sp.Resolve<TState>();
            state.SetConnectionId(cnId);
            state.BindValues();
            return state;
        }).AsSelf().Singleton();
        container.Add<TState>().AsSelf().Transient();
        container.Add(sp =>
        {
            var cfg = new ServerConfiguration();
            configure(sp, cfg);
            return cfg;
        }).AsSelf().Singleton();

        // internal
        container.Add<ServerLifetime>().AsInterfaces().Singleton();
        container.Add<ConnectionTracker>().AsSelf().Singleton();
        container.Add<BroadcastCoordinator>().AsSelf().Singleton();
        container.Add<ConnectionHandlerFactory<TState>>().AsSelf().Singleton();
        container.Add<LifeCycleCoordinator<TState>>().AsSelf().Scoped();
        container.Add(typeof(MessageHandler<>)).AsSelf().Transient();
        container.Add<PusherCoordinator<TState>>().AsSelf().Singleton();
        container.Add<Serializer>().AsSelf().Singleton();

        // internal - handlers
        container.Add<SubscriptionContextStore>().AsSelf().AsInterfaces().Singleton();
        container.AddAll()
            .AssignableTo<LifeCycleHandlerBase<TState>>()
            .As<LifeCycleHandlerBase<TState>>()
            .Scoped();

        // handlers
        container.AddBroadcasters();
        container.AddPushers<TState>();

        // models
        container.AddValueLoaders<TState>();

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

    private static void AddPushers<TState>(this IServiceContainer container)
        where TState : ConnectionStateBase
    {
        var types = container.GetTypeManager().GetImplementations(typeof(IPusher<,>));
        foreach (var type in types)
        {
            var messageType = type.GetInterfaces()
                .Single(x => x.Name == typeof(IPusher<,>).Name)
                .GetGenericArguments()[0];
            container.Add(type).AsInterfaces().Singleton();
            container.Add(typeof(PusherRunner<,>).MakeGenericType(messageType, typeof(TState)))
                .As(typeof(IPusherRunner<TState>))
                .Singleton();
        }
    }

    private static void AddValueLoaders<TState>(this IServiceContainer container)
        where TState : ConnectionStateBase
    {
        var loaderInterfaces = new HashSet<Type>();

        RegisterContainersAndLoaders(typeof(IValueLoader<,,>), typeof(ValueContainer<,,,>));
        RegisterContainersAndLoaders(typeof(IValueLoader<,>), typeof(ValueContainer<,,>));

        void RegisterContainersAndLoaders(Type valueLoader, Type valueContainer)
        {
            var loaderTypes = container.GetTypeManager().GetImplementations(valueLoader);
            foreach (var loaderType in loaderTypes)
            {
                var loaderInterface = loaderType.GetTargetImplementation(valueLoader)!;
                if (!loaderInterfaces.Add(loaderInterface))
                    throw new InvalidOperationException($"Loader {loaderInterface.FriendlyName()} is registered twice");

                var loaderArguments = loaderInterface.GetGenericArguments();
                if (loaderArguments[0] != typeof(TState))
                    continue;
                var valueContainerType = valueContainer
                    .MakeGenericType(new[] { typeof(TState), loaderType }.Concat(loaderArguments.Skip(1)).ToArray());

                container.Add(loaderType).AsInterfaces().Transient();
                container.Add(valueContainerType).AsInterfaces().Transient();
            }
        }
    }
}
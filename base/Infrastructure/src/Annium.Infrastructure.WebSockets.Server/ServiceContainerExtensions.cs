using System;
using System.Linq;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddWebSocketServer<TState>(
            this IServiceContainer container,
            Action<IServiceProvider, ServerConfiguration> configure,
            Func<Guid, TState> stateFactory
        )
            where TState : ConnectionStateBase
        {
            // public
            container.Add<ICoordinator, Coordinator<TState>>().Singleton();
            container.Add(stateFactory).AsSelf().Singleton();
            container.Add(sp =>
            {
                var cfg = new ServerConfiguration();
                configure(sp, cfg);
                return cfg;
            }).AsSelf().Singleton();

            // internal
            container.Add<BroadcastCoordinator>().AsSelf().Singleton();
            container.Add<ConnectionHandlerFactory<TState>>().AsSelf().Singleton();
            container.Add<ConnectionTracker>().AsSelf().Singleton();
            container.Add<LifeCycleCoordinator<TState>>().AsSelf().Scoped();
            container.Add<Serializer>().AsSelf().Singleton();
            container.Add<ServerLifetime>().AsInterfaces().Singleton();

            // internal - handlers
            container.Add<SubscriptionContextStore>().AsSelf().AsInterfaces().Singleton();
            container.AddAll()
                .AssignableTo<ILifeCycleHandler<TState>>()
                .AsInterfaces()
                .Scoped();

            // handlers
            container.AddBroadcasters();

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
    }
}
using System.Linq;
using System.Reflection;
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
        public static IServiceContainer AddWebSocketServer(this IServiceContainer container)
        {
            // public
            container.Add<ICoordinator, Coordinator>().Singleton();
            container.Add<ServerConfiguration>().AsSelf().Singleton();

            // internal
            container.Add<BroadcastCoordinator>().AsSelf().Singleton();
            container.Add<ConnectionHandlerFactory>().AsSelf().Singleton();
            container.Add<ConnectionTracker>().AsSelf().Singleton();
            container.Add<LifeCycleCoordinator>().AsSelf().Singleton();
            container.Add<Serializer>().AsSelf().Singleton();
            container.Add<ServerLifetime>().AsInterfaces().Singleton();
            container.Add<WorkScheduler>().AsSelf().Scoped();

            // internal - handlers
            container.Add(typeof(SubscriptionContextStore<,>)).AsSelf().Singleton();
            container.AddAll(Assembly.GetCallingAssembly(), true)
                .AssignableTo<ILifeCycleHandler>()
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
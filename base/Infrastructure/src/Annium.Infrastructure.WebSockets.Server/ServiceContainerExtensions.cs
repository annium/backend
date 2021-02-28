using System.Linq;
using Annium.Infrastructure.WebSockets.Server;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Subscriptions;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddWebSocketServer(this IServiceContainer container)
        {
            // internal
            container.Add<BroadcastCoordinator>().AsSelf().Singleton();
            container.Add<ConnectionHandlerFactory>().AsSelf().Singleton();
            container.Add<ConnectionTracker>().AsSelf().Singleton();
            container.Add<ICoordinator, Coordinator>().Singleton();
            container.Add<Serializer>().AsSelf().Singleton();
            container.Add<WorkScheduler>().AsSelf().Singleton();

            // internal - handlers
            container.Add(typeof(SubscriptionContextStore<,>)).AsSelf().Singleton();

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
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;

namespace Annium.AspNetCore.IntegrationTesting
{
    public static class WebAppFactoryWebSocketExtensions
    {
        private static readonly ConcurrentDictionary<IWebApplicationFactory, object> Cache = new();

        public static TWebSocketClient GetWebSocketClient<TWebSocketClient>(
            this IWebApplicationFactory appFactory,
            string endpoint
        )
            where TWebSocketClient : class =>
            (TWebSocketClient) Cache.GetOrAdd(appFactory, (_, factory) =>
            {
                var wsUri = new UriBuilder(appFactory.Server.BaseAddress) { Scheme = "ws", Path = endpoint }.Uri;
                var ws = appFactory.Server.CreateWebSocketClient().ConnectAsync(wsUri, CancellationToken.None).Await();

                var client = factory.ServiceProvider.Resolve<Func<WebSocket, TWebSocketClient>>()(ws)!;

                return client;
            }, appFactory);
    }
}
using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;

namespace Annium.AspNetCore.IntegrationTesting
{
    public static class WebAppFactoryWebSocketExtensions
    {
        private static readonly ConditionalWeakTable<IWebApplicationFactory, object> Cache = new();

        public static TWebSocketClient GetWebSocketClient<TWebSocketClient>(
            this IWebApplicationFactory appFactory,
            string endpoint
        )
            where TWebSocketClient : class =>
            (TWebSocketClient) Cache.GetValue(appFactory, factory =>
            {
                var wsUri = new UriBuilder(factory.Server.BaseAddress) { Scheme = "ws", Path = endpoint }.Uri;
                var ws = factory.Server.CreateWebSocketClient().ConnectAsync(wsUri, CancellationToken.None).Await();

                var client = factory.ServiceProvider.Resolve<Func<WebSocket, TWebSocketClient>>()(ws)!;

                return client;
            });
    }
}
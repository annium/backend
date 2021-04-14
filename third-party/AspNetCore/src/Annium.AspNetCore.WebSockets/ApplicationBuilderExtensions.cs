using System;
using Annium.Architecture.Http.Profiles;
using Annium.AspNetCore.WebSockets.Internal.Middleware;
using Annium.Core.Runtime;
using Annium.Infrastructure.WebSockets.Server;
using Microsoft.AspNetCore.Builder;

[assembly: ReferTypeAssembly(typeof(HttpStatusCodeProfile))]

namespace Annium.Core.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseWebSocketsServer(
            this IApplicationBuilder builder
        ) => builder.UseWebSocketsServer(_ => { });

        public static IApplicationBuilder UseWebSocketsServer(
            this IApplicationBuilder builder,
            Action<ServerConfiguration> configure
        )
        {
            var configuration = builder.ApplicationServices.Resolve<ServerConfiguration>();
            configure(configuration);

            builder.UseWebSockets();

            return builder.Map(configuration.Endpoint, x => x.UseMiddleware<WebSocketsMiddleware>());
        }
    }
}
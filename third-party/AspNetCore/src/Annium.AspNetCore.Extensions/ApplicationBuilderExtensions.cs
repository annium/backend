using Annium.AspNetCore.Extensions.Internal.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Annium.Core.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
        //
        // public static IApplicationBuilder UseWebSocketsMiddleware(this IApplicationBuilder builder, string url = "/ws")
        // {
        //     builder.UseWebSockets();
        //     return builder.Map(url, x => x.UseMiddleware<WebSocketsMiddleware>());
        // }
    }
}
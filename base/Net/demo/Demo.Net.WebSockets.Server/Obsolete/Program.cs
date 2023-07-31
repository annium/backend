using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Demo.Net.WebSockets.Server.Obsolete;

[Obsolete]
public static class Debug
{
    public static async Task RunAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Host.UseServicePack<ServicePack>();
        builder.Logging.ConfigureLoggingBridge();
        builder.WebHost.UseKestrelDefaults();

        var app = builder.Build();

        app.UseWebSockets();
        app.UseMiddleware<WebSocketEchoMiddleware>();
        app.UseRouting();

        await app.RunAsync();
    }
}
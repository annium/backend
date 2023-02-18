using System.IO;
using Microsoft.AspNetCore.Hosting;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseKestrelDefaults(this IWebHostBuilder builder, int port = 5000) => builder
        .UseKestrel(server =>
        {
            var cfg = server.ApplicationServices.Resolve<WebHostConfiguration>();
            server.AddServerHeader = false;
            server.ListenAnyIP(cfg.Port, listen =>
            {
                if (cfg.Cert is null)
                    return;

                var certFile = Path.GetFullPath(cfg.Cert);
                if (!File.Exists(certFile))
                    throw new FileNotFoundException($"Cert file {certFile} not found");
                listen.UseHttps(certFile);
            });
        });
}

public sealed record WebHostConfiguration
{
    public int Port { get; set; } = 5000;
    public string? Cert { get; set; }
}
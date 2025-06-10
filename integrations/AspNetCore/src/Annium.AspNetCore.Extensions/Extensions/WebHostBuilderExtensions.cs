using System.IO;
using Annium.Core.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Annium.AspNetCore.Extensions.Extensions;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseKestrelDefaults(this IWebHostBuilder builder, int port = 5000) =>
        builder.UseKestrel(server =>
        {
            var cfg = server.ApplicationServices.Resolve<WebHostConfiguration>();
            server.AddServerHeader = false;
            server.ListenAnyIP(
                cfg.Port,
                listen =>
                {
                    if (cfg.Cert is null)
                        return;

                    var certFile = Path.GetFullPath(cfg.Cert);
                    if (!File.Exists(certFile))
                        throw new FileNotFoundException($"Cert file {certFile} not found");
                    listen.UseHttps(certFile);
                }
            );
        });
}

public sealed record WebHostConfiguration
{
    public int Port { get; set; } = 5000;
    public string? Cert { get; set; }
}

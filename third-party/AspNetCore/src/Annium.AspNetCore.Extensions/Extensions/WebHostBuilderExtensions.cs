using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Annium.Core.DependencyInjection;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseKestrelDefaults(this IWebHostBuilder builder) => builder
        .UseKestrel(server =>
        {
            server.AddServerHeader = false;
            server.ListenAnyIP(5000, listen =>
            {
                var certFile = Path.GetFullPath(Path.Combine("certs", "cert.pfx"));
                if (File.Exists(certFile))
                    listen.UseHttps(certFile);
            });
        });
}
using System.IO;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace Annium.AspNetCore.Extensions.Extensions;

/// <summary>
/// Extension methods for configuring web host builder with default settings
/// </summary>
public static class WebHostBuilderExtensions
{
    /// <summary>
    /// Configures Kestrel with default settings including optional HTTPS support
    /// </summary>
    /// <param name="builder">The web host builder to configure</param>
    /// <param name="port">The default port to use if not specified in configuration</param>
    /// <returns>The configured web host builder</returns>
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

/// <summary>
/// Configuration settings for the web host
/// </summary>
public sealed record WebHostConfiguration
{
    /// <summary>
    /// Gets or sets the port for the web server
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the path to the SSL certificate file
    /// </summary>
    public string? Cert { get; set; }
}

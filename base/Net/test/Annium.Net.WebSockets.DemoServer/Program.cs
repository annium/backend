using System.IO;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Annium.Net.WebSockets.DemoServer
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new ServiceProviderFactory(b => b.UseServicePack<ServicePack>()))
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseKestrel(server =>
                        {
                            server.AddServerHeader = false;
                            server.ListenAnyIP(5000);
                        })
                        .UseStartup<Startup>();
                });
        }
    }
}
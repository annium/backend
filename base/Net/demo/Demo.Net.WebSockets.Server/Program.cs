using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Demo.Net.WebSockets.Server
{
    internal static class Program
    {
        internal static Task Run(string[] args, CancellationToken ct)
        {
            return CreateHostBuilder(args).Build().RunAsync(ct);
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

        public static Task<int> Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
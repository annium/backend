using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting
{
    internal class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly Action<IHostBuilder> configureHost;

        public TestWebApplicationFactory(Action<IHostBuilder> configureHost)
        {
            this.configureHost = configureHost;
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            var hostBuilder = Host.CreateDefaultBuilder();

            configureHost(hostBuilder);

            return hostBuilder
                .ConfigureLoggingBridge()
                .ConfigureWebHostDefaults(builder => builder
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<TStartup>()
                );
        }
    }
}
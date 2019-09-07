using System;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Logging
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var logger = provider.GetRequiredService<ILogger<Program>>();
            logger.Debug("debug");
            logger.Trace("trace");
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
using System;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Logging.Abstractions;

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
            var logger = provider.Resolve<ILogger<Program>>();
            logger.Debug("debug");
            logger.Trace("trace");
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
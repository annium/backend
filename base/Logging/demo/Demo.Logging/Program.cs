using System;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Group = Demo.Logging.Commands.Group;

namespace Demo.Logging
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken ct
        )
        {
            Commander.Run<Group>(provider, args, ct);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
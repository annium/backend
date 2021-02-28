using System;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;

namespace Demo.Infrastructure.WebSockets.Client
{
    internal static class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            new Commander(provider).Run<Commands.Group>(args, token);
        }

        internal static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
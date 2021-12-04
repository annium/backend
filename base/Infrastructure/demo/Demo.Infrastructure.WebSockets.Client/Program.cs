using System;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Extensions.Arguments;
using Group = Demo.Infrastructure.WebSockets.Client.Commands.Group;

namespace Demo.Infrastructure.WebSockets.Client;

internal static class Program
{
    private static void Run(
        IServiceProvider provider,
        string[] args,
        CancellationToken ct
    )
    {
        Commander.Run<Group>(provider, args, ct);
    }

    internal static int Main(string[] args) => new Entrypoint()
        .UseServicePack<ServicePack>()
        .Run(Run, args);
}
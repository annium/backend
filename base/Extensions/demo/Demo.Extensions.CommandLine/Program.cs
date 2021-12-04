using System;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Extensions.CommandLine;

namespace Demo.Extensions.CommandLine;

public class Program
{
    private static void Run(
        IServiceProvider provider,
        string[] args,
        CancellationToken ct
    )
    {
        Console.WriteLine("Hello from Demo.Extensions.Cli");
        var pass = Cli.ReadSecure("Your pass: ");
        Console.WriteLine($"Pass is !{pass}!");
    }

    public static int Main(string[] args) => new Entrypoint()
        .UseServicePack<ServicePack>()
        .Run(Run, args);
}
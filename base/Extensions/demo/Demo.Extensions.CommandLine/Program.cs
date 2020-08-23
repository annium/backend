using System;
using System.Threading;
using Annium.Extensions.CommandLine;
using Annium.Core.Entrypoint;

namespace Demo.Extensions.CommandLine
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
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
}
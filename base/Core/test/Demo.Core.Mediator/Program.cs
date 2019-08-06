using System;
using System.Threading;
using Annium.Core.Entrypoint;

namespace Demo.Core.Mediator
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            Console.WriteLine("Hello from Demo.Core.Mediator");
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
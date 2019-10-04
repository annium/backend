using System;
using System.Threading;
using Annium.Core.Entrypoint;

namespace Demo.Data.Operations.Serialization
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            Console.WriteLine("Hello from Demo.Data.Operations.Serialization");
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
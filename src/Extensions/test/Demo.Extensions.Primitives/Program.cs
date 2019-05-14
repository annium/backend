using System;
using System.Threading;
using Annium.Extensions.Entrypoint;
using Annium.Extensions.Primitives;

namespace Demo.Extensions.Primitives
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            Console.WriteLine("0".PascalCase());
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
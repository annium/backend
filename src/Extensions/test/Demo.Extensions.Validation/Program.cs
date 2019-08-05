using System;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Validation
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var validator = provider.GetRequiredService<Validator<A>>();

            var value = new A();
            var result = validator.Validate(value);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
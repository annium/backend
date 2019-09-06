using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Entrypoint;
using Annium.Extensions.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Validation
{
    public class Program
    {
        private static async Task RunAsync(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var validator = provider.GetRequiredService<IValidator<User>>();

            var value = new User();
            var result = await validator.ValidateAsync(value);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .RunAsync(RunAsync, args);
    }
}
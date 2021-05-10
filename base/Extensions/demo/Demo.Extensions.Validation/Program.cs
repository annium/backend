using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Validation;

namespace Demo.Extensions.Validation
{
    public class Program
    {
        private static async Task RunAsync(
            IServiceProvider provider,
            string[] args,
            CancellationToken ct
        )
        {
            var validator = provider.Resolve<IValidator<User>>();

            var value = new User();
            var result = await validator.ValidateAsync(value);
        }

        public static Task<int> Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(RunAsync, args);
    }
}
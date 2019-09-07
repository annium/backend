using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Entrypoint;
using Annium.Extensions.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Composition
{
    public class Program
    {
        private static async Task RunAsync(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var composer = provider.GetRequiredService<IComposer<User>>();

            var value = new User();
            var result = await composer.ComposeAsync(value);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .RunAsync(RunAsync, args);
    }
}
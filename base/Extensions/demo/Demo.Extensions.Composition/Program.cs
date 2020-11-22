using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Composition;

namespace Demo.Extensions.Composition
{
    public class Program
    {
        private static async Task Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var composer = provider.Resolve<IComposer<User>>();

            var value = new User();
            var result = await composer.ComposeAsync(value);
        }

        public static Task<int> Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
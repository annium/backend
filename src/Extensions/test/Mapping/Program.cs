using System;
using System.Threading;
using Annium.Extensions.Entrypoint;
using Annium.Extensions.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace Mapping
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var mapper = provider.GetRequiredService<IMapper>();

            var a = new A { Text = "Some Text" };
            var b = mapper.Map<B>(a);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
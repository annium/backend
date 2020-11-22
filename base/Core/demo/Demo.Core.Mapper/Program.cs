using System;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Mapper;

namespace Demo.Extensions.Mapping
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var mapper = provider.Resolve<IMapper>();

            var value = new Plain { ClientName = "Den" };
            _ = mapper.Map<Complex>(value);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
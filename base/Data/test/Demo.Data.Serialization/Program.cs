using System;
using System.Text.Json;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Data.Serialization.Tests.Json;

namespace Demo.Data.Serialization
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var options = new JsonSerializerOptions()
                .ConfigureAbstractConverter();

            Base a = new ChildA { A = 1 };
            Base b = new ChildB { B = 2 };
            var arr = new [] { a, b };
            var str = JsonSerializer.Serialize(arr, options);

            // act
            var source = JsonSerializer.Deserialize<Base[]>(str, options);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Reflection;
using Annium.Serialization.Json.Tests;

namespace Demo.Serialization.Json
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var settings = new JsonSerializerOptions()
                .ConfigureAbstractConverter();

            KeyBase a = new KeyChildA { Value = 1 };
            KeyBase b = new KeyChildB { Value = 2 };
            KeyBaseContainer<KeyBase> container = new KeyDataContainer<KeyBase> { Items = new[] { a, b } };
            var str = JsonSerializer.Serialize(container, settings);

            // act
            var result = JsonSerializer.Deserialize<KeyBaseContainer<KeyBase>>(str, settings);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
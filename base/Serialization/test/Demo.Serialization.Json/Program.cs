using System;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Serialization.Json;
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
            var serializer = StringSerializer.Default;

            KeyBase a = new KeyChildA { Value = 1 };
            KeyBase b = new KeyChildB { Value = 2 };
            KeyBaseContainer<KeyBase> container = new KeyDataContainer<KeyBase> { Items = new[] { a, b } };
            var str = serializer.Serialize(container);

            // act
            var result = serializer.Deserialize<KeyBaseContainer<KeyBase>>(str);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
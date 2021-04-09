using System;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json.Tests.Converters;

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
            var serializer = provider.Resolve<ISerializer<string>>();

            AbstractJsonConverterTest.KeyBase a = new AbstractJsonConverterTest.KeyChildA { Value = 1 };
            AbstractJsonConverterTest.KeyBase b = new AbstractJsonConverterTest.KeyChildB { Value = 2 };
            AbstractJsonConverterTest.KeyBaseContainer<AbstractJsonConverterTest.KeyBase> container =
                new AbstractJsonConverterTest.KeyDataContainer<AbstractJsonConverterTest.KeyBase> { Items = new[] { a, b } };
            var str = serializer.Serialize(container);

            // act
            var result = serializer.Deserialize<AbstractJsonConverterTest.KeyBaseContainer<AbstractJsonConverterTest.KeyBase>>(str);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Reflection;
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
            BaseContainer<Base> container = new DataContainer<Base> { Data = new[] { a, b } };
            var types = TypeManager.Instance.Types.ToList();
            var str = JsonSerializer.Serialize(container, options);

            // act
            var source = JsonSerializer.Deserialize<BaseContainer<Base>>(str, options);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}
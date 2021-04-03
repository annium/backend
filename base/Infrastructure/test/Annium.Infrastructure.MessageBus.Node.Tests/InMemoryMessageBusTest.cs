using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.MessageBus.Node.Tests
{
    public class InMemoryMessageBusTest
    {
        [Fact]
        public async Task Works()
        {
            // arrange
            var node = GetProvider().Resolve<IMessageBusNode>();
            var sink1 = new List<IX>();
            var sink2 = new List<IX>();
            node.Listen<IX>().Subscribe(sink1.Add);
            node.Listen<IX>().Subscribe(sink2.Add);
            var values = new IX[] { new A(1), new B(2) };

            // act
            foreach (var x in values)
                await node.Send(x);
            await Task.Delay(100);

            // assert
            sink1.IsEqual(values);
            sink2.IsEqual(values);
        }

        private IServiceProvider GetProvider()
        {
            var container = new ServiceContainer();
            container.AddRuntimeTools(GetType().Assembly, true);
            container.AddJsonSerializers().SetDefault();
            container.AddInMemoryMessageBus((sp, builder) => builder.WithSerializer(sp.Resolve<ISerializer<string>>()));

            var provider = container.BuildServiceProvider();

            return provider;
        }


        private record A : IX
        {
            public string Tid => GetType().GetIdString();
            public int Value { get; set; }

            public A(int value)
            {
                Value = value;
            }
        }

        private record B : IX
        {
            public string Tid => GetType().GetIdString();
            public int Value { get; set; }

            public B(int value)
            {
                Value = value;
            }
        }

        private interface IX
        {
            [ResolutionId]
            public string Tid { get; }
        }
    }
}
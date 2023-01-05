using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.MessageBus.Node.Tests;

public class InMemoryMessageBusTest
{
    [Fact]
    public async Task Works()
    {
        // arrange
        var node = GetProvider().Resolve<IMessageBusNode>();
        var sink1 = new List<IX>();
        var sink2 = new List<IX>();
        var sink3 = new List<object>();
        node.Listen<IX>().Subscribe(sink1.Add);
        node.Listen<IX>().Subscribe(sink2.Add);
        node.Listen().Subscribe(sink3.Add);
        var values = new IX[] { new A(1), new B(2) };

        // act
        foreach (var x in values)
            await node.Send(x);

        await Wait.UntilAsync(
            () =>
                sink1.Count == 2 &&
                sink2.Count == 2 &&
                sink3.Count == 2,
            new CancellationTokenSource(3000).Token
        );

        // assert
        sink1.Has(2);
        sink1.At(0).Is(values[0]);
        sink1.At(1).Is(values[1]);
        sink2.Has(2);
        sink2.At(0).Is(values[0]);
        sink2.At(1).Is(values[1]);
        sink3.Has(2);
        sink3.At(0).Is(values[0]);
        sink3.At(1).Is(values[1]);
    }

    private IServiceProvider GetProvider()
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddSerializers().WithJson(isDefault: true);
        container.AddInMemoryMessageBus((sp, builder) => builder.WithSerializer(sp.Resolve<ISerializer<string>>()));
        container.AddLogging();

        var provider = container.BuildServiceProvider();

        provider.UseLogging(route => route.UseInMemory());

        return provider;
    }


    private record A : IX
    {
        public int Value { get; }

        public A(int value)
        {
            Value = value;
        }
    }

    private record B : IX
    {
        public int Value { get; }

        public B(int value)
        {
            Value = value;
        }
    }

    private interface IX
    {
    }
}
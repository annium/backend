using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.MessageBus.Node.Tests;

/// <summary>
/// Tests for the in-memory message bus implementation, verifying message sending and receiving functionality
/// across multiple subscribers.
/// </summary>
public class InMemoryMessageBusTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryMessageBusTest"/> class with test output helper
    /// and configures the message bus with JSON serialization.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test output.</param>
    public InMemoryMessageBusTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(isDefault: true);
            container.AddInMemoryMessageBus((sp, builder) => builder.WithSerializer(sp.Resolve<ISerializer<string>>()));
        });
        Setup(sp =>
        {
            sp.UseLogging(route => route.UseInMemory());
        });
    }

    /// <summary>
    /// Tests that the message bus correctly sends messages to multiple subscribers and that all subscribers
    /// receive the same messages in the correct order.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Works()
    {
        // arrange
        var node = Get<IMessageBusNode>();
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

        await Expect.ToAsync(
            () =>
            {
                sink1.Has(2);
                sink2.Has(2);
                sink3.Has(2);
            },
            3000
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

    /// <summary>
    /// Test record implementing IX interface for message bus testing.
    /// </summary>
    private record A : IX
    {
        /// <summary>
        /// Gets the integer value for this record.
        /// </summary>
        public int Value { get; }

        public A(int value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Test record implementing IX interface for message bus testing.
    /// </summary>
    private record B : IX
    {
        /// <summary>
        /// Gets the integer value for this record.
        /// </summary>
        public int Value { get; }

        public B(int value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Marker interface for test messages in the message bus testing.
    /// </summary>
    private interface IX;
}

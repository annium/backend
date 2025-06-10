using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations.Extensions;
using Annium.Logging;
using Annium.Mesh.Tests.System.Domain;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.Mesh.Tests.Variants.Base;

/// <summary>
/// Base class for request-response mesh tests, providing common functionality for testing request-response patterns.
/// </summary>
/// <typeparam name="TBehavior">The behavior type that defines server configuration and running logic.</typeparam>
public abstract class RequestTestsBase<TBehavior> : TestBase<TBehavior>
    where TBehavior : class, IBehavior
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestTestsBase{TBehavior}"/> class with the specified test output helper.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test output.</param>
    protected RequestTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Base test method for echo request-response functionality, verifying that a message is echoed back correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task Echo_Base()
    {
        this.Trace("start");

        // arrange
        var message = "test message";

        this.Trace("get client");
        await using var client = await GetClient();

        // act
        this.Trace("send request");
        var response = await client.Demo.EchoAsync(new EchoRequest(message));

        // assert
        this.Trace("validate response");
        response.Status.Is(OperationStatus.Ok);
        response.Data.Is(message);

        this.Trace("done");
    }

    /// <summary>
    /// Base test method for concurrent echo request scenarios, sending multiple requests simultaneously and verifying responses.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    protected async Task Echo_Bundle_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("get client");
        await using var client = await GetClient();
        var responses = new ConcurrentBag<string?>();
        var range = Enumerable.Range(0, 500).Select(x => x.ToString()).ToArray();

        // act
        await Task.WhenAll(
            range.Select(async x =>
            {
                this.Trace("send request");
                var response = await client.Demo.EchoAsync(new EchoRequest(x)).GetDataAsync();
                this.Trace("add response");
                responses.Add(response);
            })
        );

        // assert
        var set = new HashSet<string?>(responses);
        set.Has(range.Length);
        foreach (var x in range)
            set.Contains(x).IsTrue();

        this.Trace("done");
    }
}

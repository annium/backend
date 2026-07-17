using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting.Http;
using Annium.AspNetCore.TestServer.Components;
using Annium.Data.Operations;
using Annium.Data.Operations.Serialization.Json;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Pins the keyed-resolution contracts of the integration testing harness: <see cref="ITestHost.GetKeyed{T}" />
/// resolving keyed services from the hosted application's own container, and the keyed overload of
/// <c>TestBaseExtensions.RegisterHttpRequestFactory</c> (the one taking a string key) wiring each key to
/// its own <see cref="ITestHost" />.
/// </summary>
public class KeyedResolutionTests : TestBase
{
    /// <summary>
    /// The key under which the HTTP request factory bound to <see cref="_hostA" /> is registered.
    /// </summary>
    private const string HostKeyA = "host-a";

    /// <summary>
    /// The key under which the HTTP request factory bound to <see cref="_hostB" /> is registered.
    /// </summary>
    private const string HostKeyB = "host-b";

    /// <summary>
    /// The first test host started by the test body; bound before the keyed HTTP request factory is resolved.
    /// </summary>
    private ITestHost _hostA = null!; // populated in the test body before the factory is resolved

    /// <summary>
    /// The second test host started by the test body; bound before the keyed HTTP request factory is resolved.
    /// </summary>
    private ITestHost _hostB = null!; // populated in the test body before the factory is resolved

    /// <summary>
    /// Initializes a new instance of the KeyedResolutionTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public KeyedResolutionTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        // Registrations must happen before InitializeAsync freezes the container, so they live here
        // in the constructor rather than the test body. Each factory binds to its host lazily.
        this.RegisterHttpRequestFactory(HostKeyA, () => _hostA);
        this.RegisterHttpRequestFactory(HostKeyB, () => _hostB);
        // Each keyed HTTP request factory resolves its own keyed serializer (keyed by the same string
        // key), so a serializer must be registered under each factory's key, not just the default one.
        Register(container => container.AddSerializers(HostKeyA).WithJson(opts => opts.ConfigureForOperations()));
        Register(container => container.AddSerializers(HostKeyB).WithJson(opts => opts.ConfigureForOperations()));
    }

    /// <summary>
    /// Tests that <see cref="ITestHost.GetKeyed{T}" /> resolves the keyed service registered under the
    /// requested key, and that two distinct keys yield two distinct, correctly-keyed instances.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task GetKeyed_TwoDistinctKeys_ResolvesDistinctKeyedInstances()
    {
        // arrange
        await using var testHost = await new KeyedTestHost(OutputHelper).StartAsync();

        // act
        var markerA = testHost.GetKeyed<IKeyedMarker>(KeyedTestHost.KeyA);
        var markerB = testHost.GetKeyed<IKeyedMarker>(KeyedTestHost.KeyB);

        // assert
        markerA.Key.Is(KeyedTestHost.KeyA);
        markerB.Key.Is(KeyedTestHost.KeyB);
        markerA.IsNot(markerB);
    }

    /// <summary>
    /// Tests that the keyed overload of <c>TestBaseExtensions.RegisterHttpRequestFactory</c> wires each
    /// key to its own <see cref="ITestHost" />: a request made through one keyed factory reaches only
    /// the server bound to that key, not the other one.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task RegisterHttpRequestFactory_TwoDistinctKeys_EachFactoryHitsExpectedServer()
    {
        // arrange
        await using var hostA = await new TestHost(OutputHelper).StartAsync();
        await using var hostB = await new TestHost(OutputHelper).StartAsync();
        _hostA = hostA;
        _hostB = hostB;
        hostA.Get<SharedDataContainer>().Value = "value-a";
        hostB.Get<SharedDataContainer>().Value = "value-b";

        // act
        var resultA = await GetKeyed<IHttpRequestFactory>(HostKeyA)
            .New()
            .Get("/")
            .AsAsync<IResult<string>>(ct: TestContext.Current.CancellationToken);
        var resultB = await GetKeyed<IHttpRequestFactory>(HostKeyB)
            .New()
            .Get("/")
            .AsAsync<IResult<string>>(ct: TestContext.Current.CancellationToken);

        // assert
        resultA.IsNotDefault();
        resultB.IsNotDefault();
        resultA.Data.Is("value-a");
        resultB.Data.Is("value-b");
    }
}

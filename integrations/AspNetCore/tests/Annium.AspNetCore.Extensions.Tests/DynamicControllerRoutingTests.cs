// The DynamicControllers.TestServer assembly is referenced under an alias (see the .csproj) because its
// top-level-statement Program class would otherwise collide with Annium.AspNetCore.TestServer's own Program
// class in the global namespace, making the unqualified `Program` reference in TestHost.cs ambiguous.
extern alias DynamicControllersTestServer;

using System.Net;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting;
using Annium.AspNetCore.IntegrationTesting.Http;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;
using ConventionalEndpointResponse = DynamicControllersTestServer::Annium.AspNetCore.DynamicControllers.TestServer.Controllers.ConventionalEndpointResponse;
using DynamicEndpointResponse = DynamicControllersTestServer::Annium.AspNetCore.DynamicControllers.TestServer.Controllers.DynamicEndpointResponse;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Pins the dynamic-controller registration and routing feature end-to-end:
/// <see cref="Annium.AspNetCore.Extensions.MvcBuilderExtensions.AddDynamicControllers" /> wires together
/// <c>DynamicControllerFeatureProvider</c> (which adds controllers marked <see cref="Microsoft.AspNetCore.Mvc.NonControllerAttribute" />,
/// so they are unreachable through any other path) and <c>DynamicControllerRouteConvention</c> (which composes
/// the route and sets the <c>area</c>/<c>controller</c>/<c>dynamicKey</c> route values). A request only
/// succeeds, and only carries the expected route values, if both collaborate correctly.
/// </summary>
public class DynamicControllerRoutingTests : TestBase
{
    /// <summary>
    /// The test host started by each test body; bound before the HTTP request factory is resolved.
    /// </summary>
    private ITestHost _testHost = null!;

    /// <summary>
    /// Initializes a new instance of the DynamicControllerRoutingTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public DynamicControllerRoutingTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        // Registrations must happen before InitializeAsync freezes the container, so they live here
        // in the constructor rather than the test body. The factory binds to the host lazily.
        this.RegisterHttpRequestFactory(() => _testHost, true);
        // The dynamic-controller test server returns plain POCOs (not Annium.Data.Operations results), so
        // only the camelCase naming policy needs to match the server's default System.Text.Json settings.
        Register(container => container.AddSerializers().WithJson(opts => opts.UseCamelCaseNamingPolicy()));
    }

    /// <summary>
    /// Tests that a dynamic controller registered without an area is reachable at its plain route, and
    /// that the convention wires the controller/dynamicKey route values without an area value
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Get_PlainDynamicController_UsesUnprefixedRoute()
    {
        // arrange
        await using var testHost = await new DynamicControllersTestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Get("/plain-dynamic")
            .AsResponseAsync<DynamicEndpointResponse>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        var data = response.Data!;
        data.Value.Is("plain");
        data.Controller.Is("Plain");
        data.Key.Is("plain-key");
        data.Area.IsDefault();
    }

    /// <summary>
    /// Tests that a dynamic controller registered with an area is reachable only at its area-prefixed
    /// route, and that the convention wires the area/controller/dynamicKey route values
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Get_AreaDynamicController_UsesAreaPrefixedRoute()
    {
        // arrange
        await using var testHost = await new DynamicControllersTestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Get("/dynamic-area/area-dynamic")
            .AsResponseAsync<DynamicEndpointResponse>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        var data = response.Data!;
        data.Value.Is("area");
        data.Controller.Is("Area");
        data.Key.Is("area-key");
        data.Area.Is("dynamic-area");
    }

    /// <summary>
    /// Tests that a dynamic controller registered with an area is NOT reachable at its route without the
    /// area prefix, proving the area segment is actually part of the composed route rather than optional
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Get_AreaDynamicController_UnprefixedRoute_NotFound()
    {
        // arrange
        await using var testHost = await new DynamicControllersTestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Get("/area-dynamic")
            .AsResponseAsync<DynamicEndpointResponse>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests that a conventional (non-dynamic) controller registered alongside <c>AddDynamicControllers</c>
    /// is reachable at its own fixed route and carries none of the dynamic-controller route values, proving
    /// that <c>DynamicControllerRouteConvention.Apply</c>'s <c>model is null</c> guard leaves it untouched
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation</returns>
    [Fact]
    public async Task Get_ConventionalController_CoexistsWithDynamicControllers_RoutesUnmodified()
    {
        // arrange
        await using var testHost = await new DynamicControllersTestHost(OutputHelper).StartAsync();
        _testHost = testHost;

        var httpRequestFactory = Get<IHttpRequestFactory>();

        // act
        var response = await httpRequestFactory
            .New()
            .Get("/conventional")
            .AsResponseAsync<ConventionalEndpointResponse>(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        var data = response.Data!;
        data.Value.Is("conventional");
        data.Controller.Is("Conventional");
        data.Area.IsDefault();
        data.Key.IsDefault();
    }
}

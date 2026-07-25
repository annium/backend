// The DynamicControllers.TestServer assembly is referenced under an alias (see the .csproj) because its
// top-level-statement Program class would otherwise collide with Annium.AspNetCore.TestServer's own Program
// class in the global namespace, making the unqualified `Program` reference in TestHost.cs ambiguous.
extern alias DynamicControllersTestServer;

using Annium.AspNetCore.IntegrationTesting;
using Annium.Infrastructure.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;
using DynamicControllersEntryPoint = DynamicControllersTestServer::Annium.AspNetCore.DynamicControllers.TestServer.EntryPoint;
using DynamicControllersServicePack = DynamicControllersTestServer::Annium.AspNetCore.DynamicControllers.TestServer.ServicePack;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Test host for the dynamic-controllers routing tests. Boots the dedicated
/// <c>Annium.AspNetCore.DynamicControllers.TestServer</c> entry point, which calls
/// <see cref="Annium.AspNetCore.Extensions.MvcBuilderExtensions.AddDynamicControllers" /> at MVC-builder time.
/// Kept separate from the shared <see cref="TestHost" />/<c>TestServicePack</c> pair used by
/// <see cref="ServerControllerTests" />, since dynamic-controller registration must happen before the MVC
/// builder is consumed and has no reason to be layered onto the shared server's controllers.
/// </summary>
internal class DynamicControllersTestHost : TestHostBase<DynamicControllersEntryPoint>
{
    public DynamicControllersTestHost(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        //
    }

    /// <summary>
    /// Configures the host builder by applying <see cref="DynamicControllersServicePack" />, which registers
    /// MVC with the dynamic controllers exercised by <see cref="DynamicControllerRoutingTests" />.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<DynamicControllersServicePack>();
    }
}

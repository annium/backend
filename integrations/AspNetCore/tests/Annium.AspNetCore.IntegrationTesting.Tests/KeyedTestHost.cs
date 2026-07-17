using Annium.AspNetCore.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Test host that additionally registers two keyed <see cref="IKeyedMarker" /> singletons directly on
/// the ASP.NET Core container (via <see cref="IHostBuilder.ConfigureServices" />), independent of the
/// shared <see cref="Annium.AspNetCore.TestServer" /> registrations, so that
/// <see cref="ITestHost.GetKeyed{T}" /> has keyed services to resolve against.
/// </summary>
internal class KeyedTestHost : TestHostBase<Program>
{
    /// <summary>
    /// The key under which the first <see cref="IKeyedMarker" /> singleton is registered.
    /// </summary>
    public const string KeyA = "marker-a";

    /// <summary>
    /// The key under which the second <see cref="IKeyedMarker" /> singleton is registered.
    /// </summary>
    public const string KeyB = "marker-b";

    /// <summary>
    /// Initializes a new instance of the KeyedTestHost class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public KeyedTestHost(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Configures the host builder by applying <see cref="TestServicePack" /> and then additionally
    /// registering two keyed <see cref="IKeyedMarker" /> singletons directly on the ASP.NET Core
    /// container. This is purely additive: it does not touch any registration used by other test hosts.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<TestServicePack>();
        builder.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IKeyedMarker>(KeyA, new KeyedMarker(KeyA));
            services.AddKeyedSingleton<IKeyedMarker>(KeyB, new KeyedMarker(KeyB));
        });
    }
}

/// <summary>
/// Marker service used to pin the keyed-resolution behavior of <see cref="ITestHost.GetKeyed{T}" />.
/// </summary>
internal interface IKeyedMarker
{
    /// <summary>
    /// Gets the key this instance was registered under.
    /// </summary>
    string Key { get; }
}

/// <summary>
/// Default implementation of <see cref="IKeyedMarker" />.
/// </summary>
internal sealed class KeyedMarker : IKeyedMarker
{
    /// <summary>
    /// Gets the key this instance was registered under.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Initializes a new instance of the KeyedMarker class
    /// </summary>
    /// <param name="key">The key this instance was registered under</param>
    public KeyedMarker(string key)
    {
        Key = key;
    }
}

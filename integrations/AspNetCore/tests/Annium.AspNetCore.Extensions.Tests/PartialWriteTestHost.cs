using Annium.AspNetCore.IntegrationTesting;
using Annium.AspNetCore.TestServer;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.Hosting;
using Annium.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Test host for <see cref="ExceptionMiddlewareTests" />. Reuses the shared
/// <see cref="Annium.AspNetCore.TestServer" /> <c>Program</c> (same as <see cref="TestHost" />) and
/// additively registers an <see cref="EscapedExceptionSink" />, a <see cref="RecordingLogger" /> override,
/// and the <see cref="EscapedExceptionStartupFilter" /> that wraps the pipeline — all via
/// <see cref="IHostBuilder.ConfigureServices" />, so the shared test server's <c>Program</c>,
/// <c>ServicePack</c> and controllers are left completely untouched.
/// </summary>
internal class PartialWriteTestHost : TestHostBase<Program>
{
    /// <summary>
    /// Records any exception that escapes the wrapped middleware chain for a request.
    /// </summary>
    public EscapedExceptionSink EscapedException { get; } = new();

    /// <summary>
    /// Records the exception passed to the last <c>ILogger.Error</c> call made by the hosted application,
    /// overriding the real <c>ILogger</c> the hosted application would otherwise resolve.
    /// </summary>
    public RecordingLogger RecordingLogger { get; } = new();

    /// <summary>
    /// Initializes a new instance of the PartialWriteTestHost class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public PartialWriteTestHost(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Configures the host builder by applying <see cref="TestServicePack" /> (the same pack used by
    /// <see cref="TestHost" />) and then additionally registering the escaped-exception sink, the recording
    /// logger override, and the wrapping <see cref="EscapedExceptionStartupFilter" />. This is purely
    /// additive: it does not touch any registration used by other test hosts.
    /// </summary>
    /// <remarks>
    /// The <see cref="RecordingLogger" /> override is registered via
    /// <see cref="Microsoft.Extensions.Hosting.HostingHostBuilderExtensions.ConfigureContainer{TContainerBuilder}" />
    /// (against Annium's own <see cref="IServiceProviderBuilder" />, the container-builder type its custom
    /// <c>IServiceProviderFactory</c> uses) rather than plain <see cref="IHostBuilder.ConfigureServices" />.
    /// <see cref="Annium.Core.DependencyInjection.ServiceProviderFactory.CreateBuilder" /> applies
    /// <see cref="TestServicePack" />'s own registrations — including the real, scoped <c>ILogger</c> added by
    /// <c>Annium.Logging.Shared</c> — synchronously while the host is being built, i.e. strictly after every
    /// plain <c>ConfigureServices</c> callback (including a same-type override) has already run. Registering
    /// the override pack via <c>ConfigureContainer</c> instead defers it until after that point, so it is
    /// added — and thus wins module resolution — after the real registration rather than before it.
    /// </remarks>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<TestServicePack>();
        builder.ConfigureContainer<IServiceProviderBuilder>(spBuilder =>
            spBuilder.UseServicePack(
                new DynamicServicePack().Register(
                    (container, _) => container.Collection.AddSingleton<ILogger>(RecordingLogger)
                )
            )
        );
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(EscapedException);
            services.AddSingleton<IStartupFilter, EscapedExceptionStartupFilter>();
        });
    }
}

using System;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting.Internal;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting;

/// <summary>
/// Base class for in-memory ASP.NET Core integration test hosts, managing the lifecycle of a
/// <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/> and exposing
/// service resolution helpers.
/// </summary>
/// <typeparam name="TEntryPoint">The entry-point class (e.g. <c>Program</c> or <c>Startup</c>) used to bootstrap the application.</typeparam>
public abstract class TestHostBase<TEntryPoint> : ITestHost, ILogSubject
    where TEntryPoint : class
{
    /// <summary>
    /// Gets the logger associated with this test host, resolved lazily from the application's service provider.
    /// </summary>
    public ILogger Logger => _logger.Value;

    /// <summary>
    /// Gets the underlying <see cref="TestServer"/> created by the in-memory application factory.
    /// </summary>
    public TestServer Server => AppFactory.Server;

    /// <summary>
    /// Gets or sets the <see cref="WebApplicationFactory{TEntryPoint}"/> that hosts the in-memory application.
    /// Throws <see cref="InvalidOperationException"/> if accessed before <see cref="StartAsync"/> is called.
    /// </summary>
    private WebApplicationFactory<TEntryPoint> AppFactory
    {
        get => field ?? throw new InvalidOperationException("TestHost is not started");
        set;
    }

    /// <summary>
    /// The xUnit output helper used to route log output to the test output stream.
    /// </summary>
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Lazily initialised logger resolved from the application's service provider after the host starts.
    /// </summary>
    private readonly Lazy<ILogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestHostBase{TEntryPoint}"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the host logs through.</param>
    protected TestHostBase(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _logger = new Lazy<ILogger>(GetLogger);
    }

    /// <summary>
    /// Creates the in-memory application factory, invokes <see cref="HandleStartAsync"/>, and returns this host.
    /// </summary>
    /// <returns>This <see cref="ITestHost"/> instance after the host has started.</returns>
    public async ValueTask<ITestHost> StartAsync()
    {
        AppFactory = new TestWebApplicationFactory<TEntryPoint>(ConfigureHostBase);

        this.Trace("starting");
        await HandleStartAsync();
        this.Trace("started");

        return this;
    }

    /// <summary>
    /// Invokes <see cref="HandleStopAsync"/>, then disposes the underlying application factory and suppresses finalisation.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when the host has been fully torn down.</returns>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        this.Trace("stopping");
        await HandleStopAsync();

        this.Trace("disposing");
        await AppFactory.DisposeAsync();

        this.Trace("disposed");
    }

    /// <summary>
    /// Creates a new asynchronous service scope.
    /// </summary>
    /// <returns>An <see cref="AsyncServiceScope"/> for managing scoped services.</returns>
    public AsyncServiceScope CreateAsyncScope()
    {
        return AppFactory.Services.CreateAsyncScope();
    }

    /// <summary>
    /// Resolves a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The resolved service instance.</returns>
    public T Get<T>()
        where T : notnull => AppFactory.Services.Resolve<T>();

    /// <summary>
    /// Resolves a keyed service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="key">The key for the service.</param>
    /// <returns>The resolved service instance.</returns>
    public T GetKeyed<T>(object key)
        where T : notnull => AppFactory.Services.ResolveKeyed<T>(key);

    /// <summary>
    /// Applies test-specific host configuration such as service overrides, additional middleware, or environment settings.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder"/> for the in-memory application.</param>
    protected abstract void ConfigureHost(IHostBuilder builder);

    /// <summary>
    /// Override to perform additional start-up work after the application factory is created.
    /// The base implementation returns a completed task.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when start-up handling is finished.</returns>
    protected virtual ValueTask HandleStartAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Override to perform tear-down work before the application factory is disposed.
    /// The base implementation returns a completed task.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when stop handling is finished.</returns>
    protected virtual ValueTask HandleStopAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Registers the xUnit output helper as a singleton service and then delegates to <see cref="ConfigureHost"/> for
    /// test-specific configuration.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder"/> for the in-memory application.</param>
    private void ConfigureHostBase(IHostBuilder builder)
    {
        builder.ConfigureServices(services => services.AddSingleton(_outputHelper));
        ConfigureHost(builder);
    }

    /// <summary>
    /// Resolves an <see cref="ILogger"/> from the started application's service provider.
    /// </summary>
    /// <returns>The <see cref="ILogger"/> registered in the application's DI container.</returns>
    private ILogger GetLogger()
    {
        return AppFactory.Services.Resolve<ILogger>();
    }
}

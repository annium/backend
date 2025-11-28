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

public abstract class TestHostBase<TEntryPoint> : ITestHost, ILogSubject
    where TEntryPoint : class
{
    public ILogger Logger => _logger.Value;

    public TestServer Server => AppFactory.Server;

    private WebApplicationFactory<TEntryPoint> AppFactory
    {
        get => field ?? throw new InvalidOperationException("TestHost is not started");
        set;
    }

    private readonly ITestOutputHelper _outputHelper;
    private readonly Lazy<ILogger> _logger;

    protected TestHostBase(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _logger = new Lazy<ILogger>(GetLogger);
    }

    public async ValueTask<ITestHost> StartAsync()
    {
        AppFactory = new TestWebApplicationFactory<TEntryPoint>(ConfigureHostBase);

        this.Trace("starting");
        await HandleStartAsync();
        this.Trace("started");

        return this;
    }

    public async ValueTask DisposeAsync()
    {
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

    protected abstract void ConfigureHost(IHostBuilder builder);

    protected virtual ValueTask HandleStartAsync()
    {
        return ValueTask.CompletedTask;
    }

    protected virtual ValueTask HandleStopAsync()
    {
        return ValueTask.CompletedTask;
    }

    private void ConfigureHostBase(IHostBuilder builder)
    {
        builder.ConfigureServices(services => services.AddSingleton(_outputHelper));
        ConfigureHost(builder);
    }

    private ILogger GetLogger()
    {
        return AppFactory.Services.Resolve<ILogger>();
    }
}

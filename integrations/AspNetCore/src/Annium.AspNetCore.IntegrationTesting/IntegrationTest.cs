using System;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting.Internal;
using Annium.Core.DependencyInjection;
using Annium.Debug;
using Annium.Testing.Lib;
using Annium.Testing.Lib.Internal;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace Annium.AspNetCore.IntegrationTesting;

public abstract class IntegrationTest : TestBase, IAsyncDisposable
{
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(NoopTracer.Instance);
    private readonly ITestOutputHelper _outputHelper;

    protected IntegrationTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    #region host configuration & setup

    private Action<IHostBuilder> ConfigureHost(
        Action<IServiceProviderBuilder> configureBuilder
    ) => ConfigureHost(configureBuilder, _ => { });

    private Action<IHostBuilder> ConfigureHost(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceContainer> configureServices
    ) => hostBuilder =>
    {
        var serviceProviderFactory = new ServiceProviderFactory(configureBuilder);
        hostBuilder.ConfigureServices((_, services) =>
        {
            var container = new ServiceContainer(services);
            configureServices(container);
            container.Add<ITracer>(new TestTracer(_outputHelper)).AsSelf().Singleton();
        });
        hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
    };

    #endregion

    #region WebAppFactory instantiation

    protected IWebApplicationFactory GetAppFactory<TStartup>(
        Action<IServiceProviderBuilder> configureBuilder
    )
        where TStartup : class =>
        GetAppFactory<TStartup>(ConfigureHost(configureBuilder), _ => { });

    protected IWebApplicationFactory GetAppFactory<TStartup>(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceContainer> configureServices
    )
        where TStartup : class =>
        GetAppFactory<TStartup>(ConfigureHost(configureBuilder, configureServices), _ => { });

    protected IWebApplicationFactory GetAppFactory<TStartup>(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceProvider> setupServices
    )
        where TStartup : class =>
        GetAppFactory<TStartup>(ConfigureHost(configureBuilder), setupServices);

    protected IWebApplicationFactory GetAppFactory<TStartup>(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceContainer> configureServices,
        Action<IServiceProvider> setupServices
    )
        where TStartup : class =>
        GetAppFactory<TStartup>(ConfigureHost(configureBuilder, configureServices), setupServices);

    private IWebApplicationFactory GetAppFactory<TEntryPoint>(
        Action<IHostBuilder> configureHost,
        Action<IServiceProvider> setupServices
    )
        where TEntryPoint : class
    {
        var appFactory = new TestWebApplicationFactory<TEntryPoint>(configureHost);
        setupServices(appFactory.Services);
        // _disposable += () => appFactory.Server.Host.StopAsync();
        var wrappedAppFactory = new WrappedWebApplicationFactory<TEntryPoint>(appFactory);
        _disposable += wrappedAppFactory;

        return wrappedAppFactory;
    }

    #endregion

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}
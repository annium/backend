using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting.Internal;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting;

public abstract class IntegrationTest : IAsyncDisposable
{
    #region host configuration

    private static Action<IHostBuilder> ConfigureHost(
        Action<IServiceProviderBuilder> configureBuilder
    ) => hostBuilder =>
    {
        var serviceProviderFactory = new ServiceProviderFactory(configureBuilder);
        hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
    };

    private static Action<IHostBuilder> ConfigureHost(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceContainer> configureServices
    ) => hostBuilder =>
    {
        var serviceProviderFactory = new ServiceProviderFactory(configureBuilder);
        hostBuilder.ConfigureServices((_, services) => configureServices(new ServiceContainer(services)));
        hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
    };

    #endregion

    #region state

    private AsyncDisposableBox _disposable = Disposable.AsyncBox();
    private readonly ConcurrentDictionary<Type, IWebApplicationFactory> _appFactoryCache = new();

    #endregion

    #region WebAppFactory cache access

    protected IWebApplicationFactory GetAppFactory<TStartup>(
        Action<IServiceProviderBuilder> configureBuilder
    )
        where TStartup : class =>
        GetAppFactory<TStartup>(ConfigureHost(configureBuilder));

    protected IWebApplicationFactory GetAppFactory<TStartup>(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceContainer> configureServices
    )
        where TStartup : class =>
        GetAppFactory<TStartup>(ConfigureHost(configureBuilder, configureServices));

    private IWebApplicationFactory GetAppFactory<TEntryPoint>(
        Action<IHostBuilder> configureHost
    )
        where TEntryPoint : class =>
        _appFactoryCache.GetOrAdd(typeof(TEntryPoint), static (_, ctx) =>
        {
            var (test, configure) = ctx;
            var appFactory = new TestWebApplicationFactory<TEntryPoint>(configure);
            // _disposable += () => appFactory.Server.Host.StopAsync();
            var wrappedAppFactory = new WrappedWebApplicationFactory<TEntryPoint>(appFactory);
            test._disposable += wrappedAppFactory;

            return wrappedAppFactory;
        }, (this, configureHost));

    #endregion

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}
using System;
using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting.Internal;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Packs;
using Annium.Core.DependencyInjection.Plugins;
using Annium.Logging;
using Annium.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting;

/// <summary>
/// Base class for integration tests that provides web application factory setup and configuration
/// </summary>
public abstract class IntegrationTest : TestBase, IAsyncDisposable
{
    /// <summary>
    /// The disposable box for managing resources that need cleanup
    /// </summary>
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// The test output helper for logging test output
    /// </summary>
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the IntegrationTest class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    protected IntegrationTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    #region host configuration & setup

    /// <summary>
    /// Configures the host builder with the specified service provider configuration
    /// </summary>
    /// <param name="configureBuilder">Action to configure the service provider builder</param>
    /// <returns>Action to configure the host builder</returns>
    private Action<IHostBuilder> ConfigureHost(Action<IServiceProviderBuilder> configureBuilder) =>
        ConfigureHost(configureBuilder, _ => { });

    /// <summary>
    /// Configures the host builder with the specified service provider and service container configuration
    /// </summary>
    /// <param name="configureBuilder">Action to configure the service provider builder</param>
    /// <param name="configureServices">Action to configure the service container</param>
    /// <returns>Action to configure the host builder</returns>
    private Action<IHostBuilder> ConfigureHost(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceContainer> configureServices
    ) =>
        hostBuilder =>
        {
            var serviceProviderFactory = new ServiceProviderFactory(configureBuilder);
            hostBuilder.ConfigureServices(
                (_, services) =>
                {
                    var container = new ServiceContainer(services);
                    configureServices(container);
                    container.Add(_outputHelper).AsSelf().Singleton();
                }
            );
            hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
        };

    /// <summary>
    /// Sets up the host with default configuration
    /// </summary>
    /// <param name="sp">The service provider</param>
    private void SetupHost(IServiceProvider sp)
    {
        SetupHost(_ => { })(sp);
    }

    /// <summary>
    /// Creates a host setup action with the specified services configuration
    /// </summary>
    /// <param name="setupServices">Action to setup services</param>
    /// <returns>Action to setup the service provider</returns>
    private Action<IServiceProvider> SetupHost(Action<IServiceProvider> setupServices) => setupServices;

    #endregion

    #region WebAppFactory instantiation

    /// <summary>
    /// Creates a web application factory with the specified startup class and service provider configuration
    /// </summary>
    /// <typeparam name="TStartup">The startup class type</typeparam>
    /// <param name="configureBuilder">Action to configure the service provider builder</param>
    /// <returns>The configured web application factory</returns>
    protected IWebApplicationFactory GetAppFactory<TStartup>(Action<IServiceProviderBuilder> configureBuilder)
        where TStartup : class => GetAppFactory<TStartup>(ConfigureHost(configureBuilder), SetupHost);

    /// <summary>
    /// Creates a web application factory with the specified startup class, service provider, and service container configuration
    /// </summary>
    /// <typeparam name="TStartup">The startup class type</typeparam>
    /// <param name="configureBuilder">Action to configure the service provider builder</param>
    /// <param name="configureServices">Action to configure the service container</param>
    /// <returns>The configured web application factory</returns>
    protected IWebApplicationFactory GetAppFactory<TStartup>(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceContainer> configureServices
    )
        where TStartup : class =>
        GetAppFactory<TStartup>(ConfigureHost(configureBuilder, configureServices), SetupHost);

    /// <summary>
    /// Creates a web application factory with the specified startup class, service provider configuration, and services setup
    /// </summary>
    /// <typeparam name="TStartup">The startup class type</typeparam>
    /// <param name="configureBuilder">Action to configure the service provider builder</param>
    /// <param name="setupServices">Action to setup services</param>
    /// <returns>The configured web application factory</returns>
    protected IWebApplicationFactory GetAppFactory<TStartup>(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceProvider> setupServices
    )
        where TStartup : class => GetAppFactory<TStartup>(ConfigureHost(configureBuilder), SetupHost(setupServices));

    /// <summary>
    /// Creates a web application factory with the specified startup class, service provider configuration, service container configuration, and services setup
    /// </summary>
    /// <typeparam name="TStartup">The startup class type</typeparam>
    /// <param name="configureBuilder">Action to configure the service provider builder</param>
    /// <param name="configureServices">Action to configure the service container</param>
    /// <param name="setupServices">Action to setup services</param>
    /// <returns>The configured web application factory</returns>
    protected IWebApplicationFactory GetAppFactory<TStartup>(
        Action<IServiceProviderBuilder> configureBuilder,
        Action<IServiceContainer> configureServices,
        Action<IServiceProvider> setupServices
    )
        where TStartup : class =>
        GetAppFactory<TStartup>(ConfigureHost(configureBuilder, configureServices), SetupHost(setupServices));

    /// <summary>
    /// Creates a web application factory with the specified entry point, host configuration, and services setup
    /// </summary>
    /// <typeparam name="TEntryPoint">The entry point class type</typeparam>
    /// <param name="configureHost">Action to configure the host builder</param>
    /// <param name="setupServices">Action to setup services</param>
    /// <returns>The configured web application factory</returns>
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

    /// <summary>
    /// Disposes the integration test and cleans up resources
    /// </summary>
    /// <returns>A value task that represents the asynchronous dispose operation</returns>
    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}

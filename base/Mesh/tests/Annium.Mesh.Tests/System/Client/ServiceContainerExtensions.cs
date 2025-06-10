using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Client;
using Annium.Mesh.Tests.System.Client.Clients;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Tests.System.Client;

/// <summary>
/// Service container extensions for registering test server client services.
/// </summary>
public static class TestServerClientServiceContainerExtensions
{
    /// <summary>
    /// Registers a test server client with the specified configuration.
    /// </summary>
    /// <param name="container">The service container to register services in.</param>
    /// <param name="configure">The action to configure the client.</param>
    /// <returns>The service container for chaining.</returns>
    public static IServiceContainer AddTestServerClient(
        this IServiceContainer container,
        Action<ClientConfiguration> configure
    )
    {
        // client
        container
            .Add(sp =>
            {
                var configuration = new ClientConfiguration();
                configure(configuration);

                var clientFactory = sp.Resolve<IClientFactory>();
                var client = clientFactory.Create(configuration);

                return new TestServerClient(client);
            })
            .AsSelf()
            .Singleton();

        // system
        container.AddMeshClient();

        return container;
    }

    /// <summary>
    /// Registers a test server managed client factory with the specified configuration.
    /// </summary>
    /// <typeparam name="TContext">The context type for client connection.</typeparam>
    /// <param name="container">The service container to register services in.</param>
    /// <param name="configure">The optional action to configure the client.</param>
    /// <returns>The service container for chaining.</returns>
    public static IServiceContainer AddTestServerManagedClient<TContext>(
        this IServiceContainer container,
        Action<ClientConfiguration>? configure = default
    )
    {
        // test client
        container
            .Add<Func<TContext, Task<TestServerManagedClient>>>(sp =>
            {
                var configuration = new ClientConfiguration();
                configure?.Invoke(configuration);

                var clientConnectionFactory = sp.Resolve<IClientConnectionFactory<TContext>>();
                var clientFactory = sp.Resolve<IClientFactory>();

                return async context =>
                {
                    var connection = await clientConnectionFactory.CreateAsync(context);
                    var client = clientFactory.Create(connection, configuration);
                    await client.WhenConnectedAsync();

                    return new TestServerManagedClient(client);
                };
            })
            .AsSelf()
            .Singleton();

        // system
        container.AddMeshClient();

        return container;
    }
}

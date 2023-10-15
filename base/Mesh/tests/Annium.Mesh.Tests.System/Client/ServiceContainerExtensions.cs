using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Client;
using Annium.Mesh.Tests.System.Client.Clients;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Tests.System.Client;

public static class TestServerClientServiceContainerExtensions
{
    public static IServiceContainer AddTestServerClient(
        this IServiceContainer container,
        Action<ClientConfiguration> configure
    )
    {
        // client
        container.Add(sp =>
        {
            var configuration = new ClientConfiguration();
            configure(configuration);

            var clientFactory = sp.Resolve<IClientFactory>();
            var client = clientFactory.Create(configuration);

            return new TestServerClient(client);
        }).AsSelf().Singleton();

        // system
        container.AddMeshClient();

        return container;
    }

    public static IServiceContainer AddTestServerManagedClient<TContext>(
        this IServiceContainer container,
        Action<ClientConfiguration>? configure = default
    )
    {
        // test client
        container.Add<Func<TContext, Task<TestServerManagedClient>>>(sp =>
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
        }).AsSelf().Singleton();

        // system
        container.AddMeshClient();

        return container;
    }
}
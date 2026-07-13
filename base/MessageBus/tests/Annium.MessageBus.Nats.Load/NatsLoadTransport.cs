using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Tests.Load.Shared;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Testcontainers.Nats;

namespace Annium.MessageBus.Nats.Load;

/// <summary>
/// The load-harness transport seam for NATS: starts a JetStream-enabled NATS container, provisions the stream capturing
/// the load subjects (the adapter never creates streams), and registers the NATS message-bus adapter.
/// </summary>
public sealed class NatsLoadTransport : ILoadTransport
{
    /// <summary>
    /// The name of the provisioned stream.
    /// </summary>
    private const string StreamName = "LOAD";

    /// <summary>
    /// The NATS container.
    /// </summary>
    private NatsContainer? _container;

    /// <summary>
    /// The resolved connection string.
    /// </summary>
    private string _connectionString = string.Empty;

    /// <summary>
    /// Gets the broker display name.
    /// </summary>
    public string BrokerName => "Nats";

    /// <summary>
    /// Starts the JetStream-enabled NATS container and provisions the stream capturing the <c>load.&gt;</c> subjects.
    /// </summary>
    /// <returns>A task that completes once the broker is ready and the stream exists.</returns>
    public async Task StartAsync()
    {
        var container = new NatsBuilder("nats:2.11").WithCommand("-js").Build();
        await container.StartAsync();
        _container = container;
        _connectionString = container.GetConnectionString();

        await using var connection = new NatsConnection(new NatsOpts { Url = _connectionString });
        await connection.ConnectAsync();
        var jetStream = new NatsJSContext(connection);
        await jetStream.CreateStreamAsync(new StreamConfig(StreamName, ["load.>"]));
    }

    /// <summary>
    /// Registers the NATS message-bus adapter into the container.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    public void Configure(IServiceContainer container) =>
        container.AddNatsMessageBus(builder => builder.Url(_connectionString));

    /// <summary>
    /// Disposes the NATS container.
    /// </summary>
    /// <returns>A task that completes once the container has been disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Tests.Shared;
using Testcontainers.RabbitMq;

namespace Annium.MessageBus.RabbitMq.Tests;

/// <summary>
/// Conformance-suite seam for the RabbitMQ transport. A single RabbitMQ container is shared across the whole adapter
/// test run (started lazily under a static gate, reaped by the Testcontainers Ryuk sidecar at process exit), and its
/// AMQP connection string is fed to <c>AddRabbitMqMessageBus</c>.
/// </summary>
public sealed class TestTransport : IMessageBusTestTransport
{
    /// <summary>
    /// Serializes lazy container creation across concurrent test-class construction.
    /// </summary>
    private static readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// The shared RabbitMQ container (created once per run).
    /// </summary>
    private static RabbitMqContainer? _container;

    /// <summary>
    /// The resolved AMQP connection string for this instance's DI configuration.
    /// </summary>
    private string _connectionString = string.Empty;

    /// <summary>
    /// Gets the shared container (for broker-control tests that simulate an outage).
    /// </summary>
    public static RabbitMqContainer Container => _container!;

    /// <summary>
    /// Starts the shared RabbitMQ container if it has not been started yet, then captures its connection string for
    /// this instance's DI configuration.
    /// </summary>
    /// <returns>A task that completes when the container is running and the connection string has been captured.</returns>
    public async ValueTask StartAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_container is null)
            {
                var container = new RabbitMqBuilder("rabbitmq:3.13").Build();
                await container.StartAsync();
                _container = container;
            }
        }
        finally
        {
            _gate.Release();
        }

        _connectionString = _container.GetConnectionString();
    }

    /// <summary>
    /// Registers the RabbitMQ message bus in the given DI container, configured against the started broker's
    /// connection string.
    /// </summary>
    /// <param name="container">The DI container.</param>
    public void Configure(IServiceContainer container) =>
        container.AddRabbitMqMessageBus(builder => builder.ConnectionUri(_connectionString));

    /// <summary>
    /// Gets the eventual-assertion timeout (ms) for this transport.
    /// </summary>
    public int DefaultTimeoutMs => 15000;

    /// <summary>
    /// Disposes the transport seam. The shared container outlives this instance and is reaped by the Testcontainers
    /// Ryuk sidecar at process exit, so there is nothing to dispose here.
    /// </summary>
    /// <returns>A completed task.</returns>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Runs a <c>rabbitmqctl</c> control command inside the shared broker container (used to simulate an outage).
    /// </summary>
    /// <param name="command">The <c>rabbitmqctl</c> subcommand (e.g. <c>stop_app</c>, <c>start_app</c>).</param>
    /// <returns>A task that completes when the command has run.</returns>
    public static async Task ControlAsync(string command)
    {
        var result = await Container.ExecAsync(new List<string> { "rabbitmqctl", command });
        if (result.ExitCode != 0)
            throw new System.InvalidOperationException(
                $"rabbitmqctl {command} failed (exit {result.ExitCode}): {result.Stderr}"
            );
    }
}

using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.MessageBus.Tests.Shared;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Testcontainers.Nats;

namespace Annium.MessageBus.Nats.Tests;

/// <summary>
/// Conformance-suite seam for the NATS transport. A single JetStream-enabled NATS container is shared across the whole
/// adapter test run (started lazily under a static gate, reaped by the Testcontainers Ryuk sidecar at process exit),
/// and its address is fed to <c>AddNatsMessageBus</c>.
/// </summary>
/// <remarks>
/// The adapter never creates streams (external provisioning), so the fixture provisions a single stream capturing the
/// subject namespaces the suite uses (<c>orders.&gt;</c>, <c>payments.&gt;</c>, <c>replay.&gt;</c>, <c>dedup.&gt;</c>,
/// <c>validated.&gt;</c>). The <c>missing.&gt;</c> namespace is deliberately left unprovisioned so the stream-validation
/// test can assert a clear error.
/// </remarks>
public sealed class TestTransport : IMessageBusTestTransport
{
    /// <summary>
    /// The name of the provisioned stream.
    /// </summary>
    public const string StreamName = "MESSAGEBUS";

    /// <summary>
    /// The subjects captured by the provisioned stream.
    /// </summary>
    public static readonly string[] StreamSubjects =
    [
        "orders.>",
        "payments.>",
        "replay.>",
        "dedup.>",
        "validated.>",
        "load.>",
    ];

    /// <summary>
    /// The name of a second stream dedicated to the sequence-replay test, so its stream sequence is isolated (a single
    /// namespace, single writer) and therefore deterministic for <c>ByStartSequence</c> positioning.
    /// </summary>
    public const string SequenceStreamName = "REPLAYSEQ";

    /// <summary>
    /// Serializes lazy container creation and stream provisioning across concurrent test-class construction.
    /// </summary>
    private static readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// The shared NATS container (created once per run).
    /// </summary>
    private static NatsContainer? _container;

    /// <summary>
    /// Whether the shared stream has been provisioned.
    /// </summary>
    private static bool _streamProvisioned;

    /// <summary>
    /// The resolved connection string for this instance's DI configuration.
    /// </summary>
    private string _connectionString = string.Empty;

    /// <summary>
    /// Gets the shared NATS container (started by <see cref="StartAsync"/>).
    /// </summary>
    public static NatsContainer Container => _container!;

    /// <summary>
    /// Starts the shared NATS container and provisions the shared streams on first call (guarded so concurrent
    /// test-class construction only does either once), and resolves this instance's connection string from it.
    /// </summary>
    /// <returns>A task that completes once the connection string is resolved.</returns>
    public async ValueTask StartAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_container is null)
            {
                var container = new NatsBuilder("nats:2.11").WithCommand("-js").Build();
                await container.StartAsync();
                _container = container;
            }

            if (!_streamProvisioned)
            {
                await ProvisionStreamAsync(_container.GetConnectionString());
                _streamProvisioned = true;
            }
        }
        finally
        {
            _gate.Release();
        }

        _connectionString = _container.GetConnectionString();
    }

    /// <summary>
    /// Registers the NATS message bus into the container, pointed at the started container's connection string.
    /// </summary>
    /// <param name="container">The service container to add services to.</param>
    public void Configure(IServiceContainer container) =>
        container.AddNatsMessageBus(builder => builder.Url(_connectionString));

    /// <summary>
    /// Gets the eventual-assertion timeout (ms) suited to the NATS container's broker latency.
    /// </summary>
    public int DefaultTimeoutMs => 15000;

    /// <summary>
    /// No-op; the shared container is reaped by the Testcontainers Ryuk sidecar at process exit.
    /// </summary>
    /// <returns>A completed task.</returns>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Provisions the shared stream capturing the suite's subject namespaces.
    /// </summary>
    /// <param name="connectionString">The NATS connection string.</param>
    /// <returns>A task that completes when the stream exists.</returns>
    private static async Task ProvisionStreamAsync(string connectionString)
    {
        await using var connection = new NatsConnection(new NatsOpts { Url = connectionString });
        await connection.ConnectAsync();
        var jetStream = new NatsJSContext(connection);
        await jetStream.CreateStreamAsync(new StreamConfig(StreamName, StreamSubjects));
        await jetStream.CreateStreamAsync(new StreamConfig(SequenceStreamName, ["replayseq.>"]));
    }
}

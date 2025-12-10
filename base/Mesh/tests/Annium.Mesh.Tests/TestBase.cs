using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Serialization.MessagePack;
using Annium.Mesh.Server;
using Annium.Mesh.Tests.System.Client.Clients;
using Annium.Mesh.Tests.System.Domain;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;
using Action = Annium.Mesh.Tests.System.Domain.Action;

namespace Annium.Mesh.Tests;

/// <summary>
/// Base class for mesh tests that provides server lifecycle management and client creation capabilities.
/// </summary>
/// <typeparam name="TBehavior">The behavior type that defines server configuration and running logic.</typeparam>
public abstract class TestBase<TBehavior> : TestBase, IAsyncDisposable
    where TBehavior : class, IBehavior
{
    /// <summary>
    /// Lazy initialization of the behavior instance.
    /// </summary>
    private readonly Lazy<TBehavior> _behavior;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase{TBehavior}"/> class with the specified test output helper.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test output.</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddMeshServer(opts => opts.WithApi<Action>(1));

            // container.AddMeshJsonSerialization();
            container.AddMeshMessagePackSerialization();
            container.AddSerializers().WithJson(isDefault: true);
            container.Add<SharedDataContainer>().AsSelf().Singleton();

            container.Add<TBehavior>().AsSelf().Singleton();
        });
        Register(TBehavior.Register);

        _behavior = new Lazy<TBehavior>(Get<TBehavior>, isThreadSafe: true);
    }

    /// <summary>
    /// Disposes the test by stopping the mesh server and cleaning up resources.
    /// </summary>
    /// <returns>A value task representing the asynchronous disposal.</returns>
    public ValueTask DisposeAsync()
    {
        return _behavior.Value.DisposeAsync();
    }

    /// <summary>
    /// Creates and connects a test server client for interacting with the mesh server.
    /// </summary>
    /// <returns>A connected test server client.</returns>
    protected async Task<TestServerClient> GetClient()
    {
        await _behavior.Value.InitializeAsync();
        var client = Get<TestServerClient>();
        await client.ConnectAsync();

        return client;
    }
}

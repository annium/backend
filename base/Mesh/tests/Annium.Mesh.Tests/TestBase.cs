using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Extensions;
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

public abstract class TestBase<TBehavior> : TestBase, IAsyncLifetime
    where TBehavior : class, IBehavior
{
    private readonly Lazy<TBehavior> _behavior;
    private readonly CancellationTokenSource _serverCts = new();
    private Task _serverTask = Task.CompletedTask;

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

    public ValueTask InitializeAsync()
    {
        _serverTask = _behavior.Value.RunServerAsync(_serverCts.Token);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _serverCts.CancelAsync();
#pragma warning disable VSTHRD003
        await _serverTask;
#pragma warning restore VSTHRD003
    }

    protected async Task<TestServerClient> GetClient()
    {
        var client = Get<TestServerClient>();
        await client.ConnectAsync();

        return client;
    }
}

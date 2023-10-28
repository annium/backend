using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Tests.System.Client.Clients;
using Annium.Mesh.Tests.System.Domain;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;
using Action = Annium.Mesh.Tests.System.Domain.Action;

namespace Annium.Mesh.Tests.Base;

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

    public Task InitializeAsync()
    {
        _serverTask = _behavior.Value.RunServer(_serverCts.Token);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _serverCts.Cancel();
        await _serverTask;
    }

    protected async Task<TestServerClient> GetClient()
    {
        var client = Get<TestServerClient>();
        await client.ConnectAsync();

        return client;
    }
}

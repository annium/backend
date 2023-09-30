using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Internal.Handlers;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal;

internal class PusherCoordinator<TState>
    where TState : ConnectionStateBase
{
    private readonly IEnumerable<IPusherRunner<TState>> _runners;

    public PusherCoordinator(
        IEnumerable<IPusherRunner<TState>> runners
    )
    {
        _runners = runners;
    }

    public Task RunAsync(TState state, CancellationToken ct) => Task.WhenAll(_runners.Select(
        x => x.RunAsync(state, ct)
    ));
}
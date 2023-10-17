using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Internal.Handlers;
using Annium.Mesh.Server.Internal.Models;

namespace Annium.Mesh.Server.Internal;

internal class PusherCoordinator
{
    private readonly IEnumerable<IPusherRunner> _runners;

    public PusherCoordinator(
        IEnumerable<IPusherRunner> runners
    )
    {
        _runners = runners;
    }

    public Task RunAsync(ConnectionState state, CancellationToken ct) => Task.WhenAll(_runners.Select(
        x => x.RunAsync(state, ct)
    ));
}
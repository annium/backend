using System;
using System.Threading;
using System.Threading.Tasks;
using Annium;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Models;
using Demo.Mesh.Domain.Responses.System;

namespace Demo.Mesh.Server.Handlers;

internal class SessionTimePusher : IPusher<SessionTimeNotification, ConnectionState>
{
    private readonly ITimeProvider _timeProvider;

    public SessionTimePusher(
        ITimeProvider timeProvider
    )
    {
        _timeProvider = timeProvider;
    }

    public async Task RunAsync(
        IPushContext<SessionTimeNotification, ConnectionState> ctx,
        CancellationToken ct
    )
    {
        var start = _timeProvider.Now;
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), ct);

            ctx.Send(new SessionTimeNotification { Duration = (_timeProvider.Now - start).ToTimeSpan() });
        }
    }
}
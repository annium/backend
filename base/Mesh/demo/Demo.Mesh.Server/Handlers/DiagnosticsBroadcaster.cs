using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Annium;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Models;
using Demo.Mesh.Domain.Responses.System;
using Humanizer;

namespace Demo.Mesh.Server.Handlers;

internal class DiagnosticsBroadcaster : IBroadcaster<DiagnosticsNotification>
{
    public async Task Run(IBroadcastContext<DiagnosticsNotification> ctx, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), ct);

            using var process = Process.GetCurrentProcess();
            ctx.Send(new DiagnosticsNotification
            {
                StartTime = process.StartTime,
                Cpu = process.TotalProcessorTime.TotalMilliseconds.FloorInt64(),
                Memory = process.WorkingSet64.Bytes().Humanize("#.#"),
            });
        }
    }
}
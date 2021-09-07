using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Models;
using Demo.Infrastructure.WebSockets.Domain.Responses.System;
using Humanizer;

namespace Demo.Infrastructure.WebSockets.Server.Handlers
{
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
                    Cpu = (long)Math.Floor(process.TotalProcessorTime.TotalMilliseconds),
                    Memory = process.WorkingSet64.Bytes().Humanize("#.#"),
                });
            }
        }
    }
}
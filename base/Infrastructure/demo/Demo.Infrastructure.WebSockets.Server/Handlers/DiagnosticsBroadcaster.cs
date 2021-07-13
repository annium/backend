using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Models;
using Demo.Infrastructure.WebSockets.Domain.Responses.System;
using Humanizer;

namespace Demo.Infrastructure.WebSockets.Server.Handlers
{
    internal class DiagnosticsBroadcaster : IBroadcaster<DiagnosticsNotification>
    {
        public async Task Run(IBroadcastContext<DiagnosticsNotification> context)
        {
            while (!context.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                using var process = Process.GetCurrentProcess();
                context.Send(new DiagnosticsNotification
                {
                    StartTime = process.StartTime,
                    Cpu = (long) Math.Floor(process.TotalProcessorTime.TotalMilliseconds),
                    Memory = process.WorkingSet64.Bytes().Humanize("#.#"),
                });
            }
        }
    }
}
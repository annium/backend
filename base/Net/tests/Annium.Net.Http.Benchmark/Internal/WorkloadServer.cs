using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers;

namespace Annium.Net.Http.Benchmark.Internal;

internal static class WorkloadServer
{
    public static async Task RunAsync(CancellationToken ct, ILogger logger)
    {
        var server = WebServerBuilder.New(new Uri($"http://127.0.0.1:{Constants.Port}")).WithHttp(HandleHttpRequest).Build(logger);
        await server.RunAsync(ct);
    }

    private static Task HandleHttpRequest(HttpListenerContext ctx, ILogger logger, CancellationToken ct)
    {
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.Close();

        return Task.CompletedTask;
    }
}
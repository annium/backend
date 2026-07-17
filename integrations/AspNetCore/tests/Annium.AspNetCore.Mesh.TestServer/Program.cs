using System;
using Annium.AspNetCore.Mesh;
using Annium.AspNetCore.Mesh.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServicePack<ServicePack>();

var app = builder.Build();

// Test-only wrapper: fires RequestCompletionSignal (if the current test host registered one) once the
// downstream chain — including WebSocketsMiddleware — has run to completion for this request, whether it
// returned normally or faulted. See RequestCompletionSignal for why this gives tests a deterministic,
// race-free "the middleware is done" signal instead of a bounded timer.
//
// The catch here is a safety net, not a required swallow: WebSocketsMiddleware's catch-all branch is expected
// to handle every failure internally (writing an HTTP response pre-upgrade, or attempting a graceful
// WebSocket close post-upgrade) without ever letting an exception escape InvokeAsync. If one ever does escape
// regardless — e.g. a regression — it is recorded on EscapedExceptionSink (if the current test host
// registered one) instead of disappearing silently, so tests can assert on it directly.
app.Use(
    async (context, next) =>
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            context.RequestServices.GetService<EscapedExceptionSink>()?.Record(ex);
        }
        context.RequestServices.GetService<RequestCompletionSignal>()?.SignalCompleted();
    }
);

app.UseMeshWebSocketsMiddleware();

// Terminal middleware standing in for "the rest of the pipeline". Its distinct body proves a request that
// does not match WebSocketsMiddlewareConfiguration.PathMatch was passed through via `next(context)` rather
// than being handled (or swallowed) by the mesh middleware.
app.Run(context => context.Response.WriteAsync("passed-through"));

await app.RunAsync();

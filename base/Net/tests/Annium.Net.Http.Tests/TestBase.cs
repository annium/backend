using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Servers;
using Xunit.Abstractions;

namespace Annium.Net.Http.Tests;

public abstract class TestBase : Testing.Lib.TestBase
{
    private static int _basePort = 14000;
    protected readonly Uri ServerUri;
    private readonly int _port;

    protected TestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(container => container.AddHttpRequestFactory(true));
        _port = Interlocked.Increment(ref _basePort);
        ServerUri = new Uri($"ws://127.0.0.1:{_port}");
    }

    protected IAsyncDisposable RunServerBase(
        Func<HttpListenerContext, ILogger, CancellationToken, Task> handle
    )
    {
        var uri = new Uri($"http://127.0.0.1:{_port}");
        var server = WebServerBuilder.New(uri).WithHttp(handle).Build(Get<ILogger>());
        var cts = new CancellationTokenSource();
        var serverTask = server.RunAsync(cts.Token);

        return Disposable.Create(async () =>
        {
            // await before cancellation for a while
            await Task.Delay(5, CancellationToken.None);
            cts.Cancel();
            await serverTask;
        });
    }
}
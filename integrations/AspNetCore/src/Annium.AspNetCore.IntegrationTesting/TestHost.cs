using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting;

public class TestHost<TProgram> : IAsyncDisposable
    where TProgram : class
{
    public TestServer Server => _server.Value;
    private readonly Lazy<TestServer> _server;
    private readonly ITestOutputHelper _outputHelper;

    protected TestHost(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _server = new Lazy<TestServer>(CreateServer, isThreadSafe: true);
    }

    public ValueTask DisposeAsync()
    {
        return _server.Value.DisposeAsync();
    }

    protected virtual void ConfigureHost(IHostBuilder builder)
    {
        //
    }

    private TestServer CreateServer()
    {
        var builder = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(_outputHelper);
            })
            .ConfigureWebHostDefaults(webHost =>
            {
                webHost.UseTestServer();
                webHost.UseStartup<TProgram>();
            });
        ConfigureHost(builder);

        var host = builder.Start();

        return host.GetTestServer();
    }
}

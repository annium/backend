using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;

namespace Annium.AspNetCore.TestServer;

/// <summary>
/// Service pack for production test server configuration
/// </summary>
public class ServicePack : ServicePackBase
{
    /// <summary>
    /// Initializes a new instance of the ServicePack class
    /// </summary>
    public ServicePack()
    {
        Add<BaseServicePack>();
    }

    /// <summary>
    /// Registers production-specific services for the test server
    /// </summary>
    /// <param name="container">The service container to register services with</param>
    /// <param name="provider">The service provider for dependency resolution</param>
    /// <param name="ct">The cancellation token.</param>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddTime().WithRealTime().SetDefault();
        return Task.CompletedTask;
    }
}

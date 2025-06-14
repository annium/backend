using System;
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
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRealTime().SetDefault();
    }
}

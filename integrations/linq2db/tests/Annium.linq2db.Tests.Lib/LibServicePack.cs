using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;

namespace Annium.linq2db.Tests.Lib;

/// <summary>
/// Service pack for registering test library dependencies including time management and JSON serialization.
/// </summary>
public class LibServicePack : ServicePackBase
{
    /// <summary>
    /// Registers services required by the test library.
    /// </summary>
    /// <param name="container">The service container to register services with.</param>
    /// <param name="provider">The service provider for resolving dependencies.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous registration.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddTime().WithManagedTime().SetDefault();
        container.AddSerializers().WithJson(opts => opts.UseCamelCaseNamingPolicy());
        return Task.CompletedTask;
    }
}

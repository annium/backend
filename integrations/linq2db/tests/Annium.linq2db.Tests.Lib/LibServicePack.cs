using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Packs;
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
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithManagedTime().SetDefault();
        container.AddSerializers().WithJson(opts => opts.UseCamelCaseNamingPolicy());
    }
}

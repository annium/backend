using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Storage.Abstractions;

namespace Annium.Storage.Tests.Lib;

/// <summary>
/// Shared wiring for storage test suites, so each suite states only which storage it is testing.
/// </summary>
public static class TestServices
{
    /// <summary>
    /// Builds a storage instance, leaving the caller to register the storage under test.
    /// </summary>
    /// <param name="registerStorage">Registers the storage provider being tested.</param>
    /// <returns>The registered storage instance.</returns>
    public static IStorage BuildStorage(Action<IServiceContainer> registerStorage)
    {
        var services = new ServiceContainer();
        services.AddLogging();
        services.AddTime().WithManagedTime().SetDefault();
        registerStorage(services);

        var provider = services.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());

        return provider.Resolve<IStorage>();
    }
}

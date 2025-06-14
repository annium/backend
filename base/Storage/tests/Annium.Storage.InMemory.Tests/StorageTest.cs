using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Storage.Abstractions;
using Annium.Storage.Tests.Lib;

namespace Annium.Storage.InMemory.Tests;

/// <summary>
/// Test class for in-memory storage implementation.
/// Inherits from StorageTestBase to run common storage tests against the in-memory provider.
/// </summary>
public class StorageTest : StorageTestBase
{
    /// <summary>
    /// Creates and configures an in-memory storage instance for testing.
    /// Sets up dependency injection container with in-memory storage provider.
    /// </summary>
    /// <returns>A configured in-memory storage instance.</returns>
    protected override IStorage GetStorage()
    {
        var services = new ServiceContainer();
        services.AddLogging();
        services.AddTime().WithManagedTime().SetDefault();
        services.AddInMemoryStorage("default", true);

        var provider = services.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());

        return provider.Resolve<IStorage>();
    }
}

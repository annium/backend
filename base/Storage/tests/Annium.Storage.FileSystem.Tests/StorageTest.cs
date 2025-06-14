using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Storage.Abstractions;
using Annium.Storage.Tests.Lib;

namespace Annium.Storage.FileSystem.Tests;

/// <summary>
/// Test class for file system-based storage implementation.
/// Inherits from StorageTestBase to run common storage tests against the file system provider.
/// </summary>
public class StorageTest : StorageTestBase, IAsyncDisposable
{
    /// <summary>
    /// The temporary directory path used for file system storage testing.
    /// </summary>
    private readonly string _directory = Path.Combine(Path.GetTempPath().TrimEnd('/'), Guid.NewGuid().ToString());

    /// <summary>
    /// Creates and configures a file system storage instance for testing.
    /// Sets up a temporary directory and dependency injection container.
    /// </summary>
    /// <returns>A configured file system storage instance.</returns>
    protected override IStorage GetStorage()
    {
        var services = new ServiceContainer();
        services.AddLogging();
        services.AddTime().WithManagedTime().SetDefault();
        Directory.CreateDirectory(_directory);
        services.AddFileSystemStorage("default", (_, _) => new Configuration { Directory = _directory }, true);

        var provider = services.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());

        return provider.Resolve<IStorage>();
    }

    /// <summary>
    /// Cleans up the temporary directory used during testing.
    /// </summary>
    /// <returns>A completed ValueTask.</returns>
    public ValueTask DisposeAsync()
    {
        Directory.Delete(_directory, true);
        return ValueTask.CompletedTask;
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
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
        Directory.CreateDirectory(_directory);

        return TestServices.BuildStorage(services =>
            services.AddFileSystemStorage("default", (_, _) => new Configuration { Directory = _directory }, true)
        );
    }

    /// <summary>
    /// Cleans up the temporary directory used during testing.
    /// </summary>
    /// <returns>A completed ValueTask.</returns>
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        Directory.Delete(_directory, true);
        return ValueTask.CompletedTask;
    }
}

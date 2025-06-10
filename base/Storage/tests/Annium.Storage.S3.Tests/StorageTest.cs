using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Storage.Abstractions;
using Annium.Storage.Base.Tests;

namespace Annium.Storage.S3.Tests;

/// <summary>
/// Test class for S3-compatible storage implementation.
/// Inherits from StorageTestBase to run common storage tests against the S3 provider.
/// </summary>
internal class StorageTest : StorageTestBase, IAsyncDisposable
{
    /// <summary>
    /// Creates and configures an S3 storage instance for testing.
    /// Sets up dependency injection container with S3 storage provider and test configuration.
    /// </summary>
    /// <returns>A configured S3 storage instance.</returns>
    protected override IStorage GetStorage()
    {
        var services = new ServiceContainer();
        services.AddLogging();
        services.AddTime().WithManagedTime().SetDefault();
        var config = new Configuration
        {
            Server = "https://s3.yandexcloud.net",
            AccessKey = "AccessKey",
            AccessSecret = "AccessSecret",
            Region = "us-east-1",
            Bucket = "annium.tests",
            Directory = "/files",
        };
        services.AddS3Storage("default", (_, _) => config, true);

        var provider = services.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());

        return provider.Resolve<IStorage>();
    }

    /// <summary>
    /// Cleans up all test data from the S3 storage by deleting all stored items.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous cleanup operation.</returns>
    public async ValueTask DisposeAsync()
    {
        var storage = GetStorage();
        foreach (var item in await storage.ListAsync())
            await storage.DeleteAsync(item);
    }
}

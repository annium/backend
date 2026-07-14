using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Storage.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Storage.S3.Tests;

/// <summary>
/// Tests for the S3 configuration guards, which reject an incomplete or malformed configuration
/// before any request reaches the server. These need no backend, so they are kept out of the
/// MinIO-backed suites.
/// </summary>
public class ConfigurationTest
{
    /// <summary>
    /// Tests that a configuration without an access key is rejected.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task MissingAccessKey_ThrowsArgumentException()
    {
        // arrange
        var storage = GetStorage(new Configuration { AccessSecret = "secret", Bucket = "bucket" });

        // assert
        await Wrap.It(async () => await storage.ListAsync())
            .ThrowsAsync<ArgumentException>()
            .ReportsExactlyAsync("Access key is required");
    }

    /// <summary>
    /// Tests that a configuration without an access secret is rejected.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task MissingAccessSecret_ThrowsArgumentException()
    {
        // arrange
        var storage = GetStorage(new Configuration { AccessKey = "key", Bucket = "bucket" });

        // assert
        await Wrap.It(async () => await storage.UploadAsync(new MemoryStream([1]), "upload_test"))
            .ThrowsAsync<ArgumentException>()
            .ReportsExactlyAsync("Access secret is required");
    }

    /// <summary>
    /// Tests that a configuration without a bucket is rejected.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task MissingBucket_ThrowsArgumentException()
    {
        // arrange
        var storage = GetStorage(new Configuration { AccessKey = "key", AccessSecret = "secret" });

        // assert
        await Wrap.It(async () => await storage.DeleteAsync("delete_test"))
            .ThrowsAsync<ArgumentException>()
            .ReportsExactlyAsync("Bucket name is required");
    }

    /// <summary>
    /// Tests that a configuration whose directory is not absolute is rejected on construction,
    /// before any storage operation is attempted.
    /// </summary>
    [Fact]
    public void RelativeDirectory_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => GetStorage(new Configuration { Bucket = "bucket", Directory = "files" }))
            .Throws<ArgumentException>();
    }

    /// <summary>
    /// Tests that a configuration whose directory has a trailing slash is rejected on construction.
    /// </summary>
    [Fact]
    public void TrailingSlashDirectory_ThrowsArgumentException()
    {
        // assert
        Wrap.It(() => GetStorage(new Configuration { Bucket = "bucket", Directory = "/files/" }))
            .Throws<ArgumentException>();
    }

    /// <summary>
    /// Creates an S3 storage instance from the given configuration, defaulting only the region and,
    /// when the test does not set one, the directory — so each test trips exactly the guard it targets.
    /// </summary>
    /// <param name="configuration">The configuration under test.</param>
    /// <returns>A configured S3 storage instance.</returns>
    private static IStorage GetStorage(Configuration configuration)
    {
        var config = configuration with
        {
            Region = "us-east-1",
            Directory = configuration.Directory == string.Empty ? "/" : configuration.Directory,
        };

        var services = new ServiceContainer();
        services.AddLogging();
        services.AddTime().WithManagedTime().SetDefault();
        services.AddS3Storage("default", (_, _) => config, true);

        var provider = services.BuildServiceProvider();
        provider.UseLogging(x => x.UseInMemory());

        return provider.Resolve<IStorage>();
    }
}

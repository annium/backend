using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Annium.Storage.Abstractions;
using Annium.Storage.Tests.Lib;
using Annium.Testing;
using Testcontainers.Minio;
using Xunit;

namespace Annium.Storage.S3.Tests;

/// <summary>
/// Base for S3 storage tests, backed by a MinIO container so the suite needs no live cloud
/// endpoint or credentials. Each concrete suite owns a distinct bucket, so suites stay isolated
/// while sharing one container.
/// </summary>
public abstract class S3StorageTestBase : StorageTestBase, IAsyncLifetime
{
    /// <summary>
    /// Guards lazy creation of the assembly-wide MinIO container.
    /// </summary>
    private static readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// The MinIO container shared by every test in this assembly, started on first use.
    /// </summary>
    private static MinioContainer? _container;

    /// <summary>
    /// The S3 configuration pointing at the started MinIO container.
    /// </summary>
    private Configuration _configuration = new();

    /// <summary>
    /// The bucket this suite operates in. Distinct per suite to keep suites isolated.
    /// </summary>
    protected abstract string Bucket { get; }

    /// <summary>
    /// The directory prefix within the bucket this suite stores under.
    /// </summary>
    protected abstract string Root { get; }

    /// <summary>
    /// Starts the shared MinIO container (once per assembly) and ensures this suite's bucket exists.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous initialization.</returns>
    public async ValueTask InitializeAsync()
    {
        var ct = TestContext.Current.CancellationToken;

        await _gate.WaitAsync(ct);
        try
        {
            if (_container is null)
            {
                // Testcontainers' default MinIO (RELEASE.2023-01-31) predates the request checksums
                // AWSSDK v4 sends by default and rejects them; this release understands them
                var container = new MinioBuilder("minio/minio:RELEASE.2025-09-07T16-13-09Z").Build();
                await container.StartAsync(ct);
                _container = container;
            }
        }
        finally
        {
            _gate.Release();
        }

        _configuration = new Configuration
        {
            Server = _container.GetConnectionString(),
            AccessKey = _container.GetAccessKey(),
            AccessSecret = _container.GetSecretKey(),
            Region = "us-east-1",
            Bucket = Bucket,
            Directory = Root,
            ForcePathStyle = true,
        };

        using var s3 = GetRawClient();
        try
        {
            await s3.PutBucketAsync(new PutBucketRequest { BucketName = Bucket }, ct);
        }
        catch (AmazonS3Exception e) when (e.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists")
        {
            // bucket survives from an earlier test in this suite — reuse it
        }
    }

    /// <summary>
    /// Tests that listing returns every stored item, including past the page size the S3 API
    /// returns per request.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task List_BeyondSinglePage_ReturnsAll()
    {
        // arrange: more items than a page holds, so listing must follow the continuation
        const int pageSize = 10;
        const int count = 25;
        var storage = GetStorage(_configuration with { PageSize = pageSize });
        for (var i = 0; i < count; i++)
            await storage.UploadAsync(new MemoryStream([(byte)i]), $"list_paged/{i:D3}");

        // act
        var keys = await storage.ListAsync("list_paged");

        // assert
        keys.Has(count);
    }

    /// <summary>
    /// Tests that a failed download request surfaces as itself, rather than being reported as a
    /// missing item — the caller must be able to tell "not there" from "the request did not work".
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Download_RequestRejected_SurfacesError()
    {
        // arrange: credentials that pass the configuration guards but the server refuses
        var storage = GetStorage(_configuration with { AccessSecret = "wrong-secret" });

        // assert
        await Wrap.It(async () => await storage.DownloadAsync("download_rejected")).ThrowsAsync<AmazonS3Exception>();
    }

    /// <summary>
    /// Tests that a failed delete request surfaces as itself, rather than being reported as a
    /// missing item by returning false.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Delete_RequestRejected_SurfacesError()
    {
        // arrange: credentials that pass the configuration guards but the server refuses
        var storage = GetStorage(_configuration with { AccessSecret = "wrong-secret" });

        // assert
        await Wrap.It(async () => await storage.DeleteAsync("delete_rejected")).ThrowsAsync<AmazonS3Exception>();
    }

    /// <summary>
    /// Tests that a bucket that is not there reads as the item being absent, the same as a missing
    /// item in a bucket that exists — the caller asked for something that is not in storage either way.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Download_MissingBucket_ThrowsKeyNotFoundException()
    {
        // arrange: a bucket no suite provisions
        var storage = GetStorage(_configuration with { Bucket = "missing-bucket-never-provisioned" });

        // assert
        await Wrap.It(async () => await storage.DownloadAsync("download_missing_bucket"))
            .ThrowsAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Tests that deleting from a bucket that is not there reports nothing was deleted, rather than
    /// surfacing the absent bucket as a failed request.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Delete_MissingBucket_ReturnsFalse()
    {
        // arrange: a bucket no suite provisions
        var storage = GetStorage(_configuration with { Bucket = "missing-bucket-never-provisioned" });

        // act
        var result = await storage.DeleteAsync("delete_missing_bucket");

        // assert
        result.IsFalse();
    }

    /// <summary>
    /// Creates and configures an S3 storage instance for testing, using this suite's MinIO-backed configuration.
    /// </summary>
    /// <returns>A configured S3 storage instance.</returns>
    protected override IStorage GetStorage() => GetStorage(_configuration);

    /// <summary>
    /// Creates an S3 storage instance from the given configuration, letting a test vary it.
    /// </summary>
    /// <param name="configuration">The configuration to build the storage from.</param>
    /// <returns>A configured S3 storage instance.</returns>
    private static IStorage GetStorage(Configuration configuration) =>
        TestServices.BuildStorage(services => services.AddS3Storage("default", (_, _) => configuration, true));

    /// <summary>
    /// Cleans up all test data from this suite's bucket, so each test observes only what it stored.
    /// The container itself is reaped by the Testcontainers Ryuk sidecar at process exit.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous cleanup operation.</returns>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        var storage = GetStorage();
        foreach (var item in await storage.ListAsync())
            await storage.DeleteAsync(item);
    }

    /// <summary>
    /// Creates a raw S3 client against the MinIO container, used for bucket provisioning.
    /// </summary>
    /// <returns>An AmazonS3Client bound to the container.</returns>
    private AmazonS3Client GetRawClient()
    {
        var credentials = new BasicAWSCredentials(_configuration.AccessKey, _configuration.AccessSecret);
        var s3Cfg = new AmazonS3Config
        {
            ServiceURL = _configuration.Server,
            AuthenticationRegion = _configuration.Region,
            ForcePathStyle = true,
        };

        return new AmazonS3Client(credentials, s3Cfg);
    }
}

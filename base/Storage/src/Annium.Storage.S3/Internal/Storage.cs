using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Annium.Logging;
using Annium.Storage.Abstractions;
using static Annium.Storage.Abstractions.StorageHelper;

namespace Annium.Storage.S3.Internal;

/// <summary>
/// S3-compatible storage implementation of the IStorage interface that stores files in AWS S3 or S3-compatible services
/// </summary>
internal class Storage : IStorage, ILogSubject
{
    /// <summary>
    /// Logger instance for this storage implementation
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The configuration containing S3 connection details
    /// </summary>
    private readonly Configuration _configuration;

    /// <summary>
    /// The directory prefix within the S3 bucket where files are stored
    /// </summary>
    private readonly string _directory;

    /// <summary>
    /// Initializes a new instance of the S3 storage with the specified configuration
    /// </summary>
    /// <param name="configuration">The configuration containing S3 connection details</param>
    /// <param name="logger">The logger instance for logging operations</param>
    public Storage(Configuration configuration, ILogger logger)
    {
        VerifyRoot(configuration.Directory);

        _configuration = configuration;
        _directory = configuration.Directory == "/" ? string.Empty : configuration.Directory.TrimStart('/');
        Logger = logger;
    }

    /// <summary>
    /// Lists all files in the storage with an optional prefix filter
    /// </summary>
    /// <param name="prefix">Optional prefix to filter files. Empty string returns all files</param>
    /// <returns>Array of file paths matching the prefix</returns>
    public async Task<string[]> ListAsync(string prefix = "")
    {
        VerifyPrefix(prefix);

        var listRequest = new ListObjectsV2Request
        {
            BucketName = _configuration.Bucket,
            MaxKeys = _configuration.PageSize,
            Prefix = _directory == "" ? prefix : $"{_directory}/{prefix}",
        };

        using var s3 = GetClient();

        var objects = new List<S3Object>();
        ListObjectsV2Response response;
        do
        {
            response = await s3.ListObjectsV2Async(listRequest);
            // an empty listing yields a null collection, not an empty one
            objects.AddRange(response.S3Objects ?? []);
            // a listing is returned one page at a time; keep asking until the last page
            listRequest.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated ?? false);

        var shift = _directory == "" ? 0 : _directory.Length + 1;
        var keys = objects.Select(x => x.Key[shift..]);

        // the request prefix matches raw characters, so narrow it to whole path segments and keep
        // the prefix meaning the same here as in the other storages
        if (prefix != "")
            keys = keys.Where(x => x == prefix || x.StartsWith($"{prefix}/"));

        return keys.ToArray();
    }

    /// <summary>
    /// Uploads a stream to the specified path in storage
    /// </summary>
    /// <param name="source">The stream containing the data to upload</param>
    /// <param name="path">The destination path in storage</param>
    /// <returns>A task that represents the asynchronous upload operation</returns>
    public async Task UploadAsync(Stream source, string path)
    {
        VerifyPath(path);

        source.Position = 0;
        var putRequest = new PutObjectRequest
        {
            BucketName = _configuration.Bucket,
            Key = GetKey(path),
            InputStream = source,
        };

        using var s3 = GetClient();

        await s3.PutObjectAsync(putRequest);
    }

    /// <summary>
    /// Downloads a file from storage as a stream
    /// </summary>
    /// <param name="path">The path of the file to download</param>
    /// <returns>A stream containing the file content</returns>
    public async Task<Stream> DownloadAsync(string path)
    {
        VerifyPath(path);

        try
        {
            using var s3 = GetClient();

            var getRequest = new GetObjectRequest { BucketName = _configuration.Bucket, Key = GetKey(path) };
            using var getResponse = await s3.GetObjectAsync(getRequest);

            var ms = new MemoryStream();
            await getResponse.ResponseStream.CopyToAsync(ms);
            ms.Position = 0;

            return ms;
        }
        catch (AmazonS3Exception e) when (IsNotFound(e))
        {
            throw new KeyNotFoundException($"{path} not found in storage");
        }
    }

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="path">The path of the file to delete</param>
    /// <returns>True if the file was deleted, false if it did not exist</returns>
    public async Task<bool> DeleteAsync(string path)
    {
        VerifyPath(path);

        using var s3 = GetClient();

        // a HEAD answers whether the object is there without fetching its contents
        var headRequest = new GetObjectMetadataRequest { BucketName = _configuration.Bucket, Key = GetKey(path) };

        try
        {
            await s3.GetObjectMetadataAsync(headRequest);
        }
        catch (AmazonS3Exception e) when (IsNotFound(e))
        {
            return false;
        }

        var deleteRequest = new DeleteObjectRequest { BucketName = _configuration.Bucket, Key = GetKey(path) };
        await s3.DeleteObjectAsync(deleteRequest);

        return true;
    }

    /// <summary>
    /// Determines whether a failure means the object is absent, as opposed to the request itself failing
    /// (denied, throttled, server error) — which must surface rather than read as a missing object
    /// </summary>
    /// <param name="e">The exception raised by the S3 request</param>
    /// <returns>True if the object is absent</returns>
    private static bool IsNotFound(AmazonS3Exception e)
    {
        return e.StatusCode == HttpStatusCode.NotFound || e.ErrorCode is "NoSuchKey" or "NoSuchBucket" or "NotFound";
    }

    /// <summary>
    /// Creates and configures an Amazon S3 client using the provided configuration
    /// </summary>
    /// <returns>A configured AmazonS3Client instance</returns>
    private AmazonS3Client GetClient()
    {
        if (string.IsNullOrWhiteSpace(_configuration.AccessKey))
            throw new ArgumentException("Access key is required");

        if (string.IsNullOrWhiteSpace(_configuration.AccessSecret))
            throw new ArgumentException("Access secret is required");

        if (string.IsNullOrWhiteSpace(_configuration.Bucket))
            throw new ArgumentException("Bucket name is required");

        var s3Cfg = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_configuration.Region),
            RetryMode = RequestRetryMode.Adaptive,
            ForcePathStyle = _configuration.ForcePathStyle,
        };
        if (!string.IsNullOrWhiteSpace(_configuration.Server))
        {
            // ServiceURL and RegionEndpoint are mutually exclusive; keep Region as the signing region
            s3Cfg.ServiceURL = _configuration.Server;
            s3Cfg.AuthenticationRegion = _configuration.Region;
        }

        var credentials = new BasicAWSCredentials(_configuration.AccessKey, _configuration.AccessSecret);

        return new AmazonS3Client(credentials, s3Cfg);
    }

    /// <summary>
    /// Combines the configured directory prefix with the given file name to create the full S3 object key
    /// </summary>
    /// <param name="name">The file name to combine with the directory prefix</param>
    /// <returns>The full S3 object key</returns>
    private string GetKey(string name)
    {
        return _directory == string.Empty ? name : Path.Combine(_directory, name);
    }
}

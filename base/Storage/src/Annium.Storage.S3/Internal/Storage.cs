using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        var listRequest = new ListObjectsRequest
        {
            BucketName = _configuration.Bucket,
            MaxKeys = 100,
            Prefix = _directory == "" ? prefix : $"{_directory}/{prefix}",
        };

        using var s3 = GetClient();

        var objects = (await s3.ListObjectsAsync(listRequest)).S3Objects;

        if (_directory == "")
            return objects.Select(x => x.Key).ToArray();

        var shift = _directory.Length + 1;
        return objects.Select(x => x.Key[shift..]).ToArray();
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
        catch (AmazonS3Exception)
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

        var getRequest = new GetObjectRequest { BucketName = _configuration.Bucket, Key = GetKey(path) };

        try
        {
            await s3.GetObjectAsync(getRequest);
        }
        catch (AmazonS3Exception)
        {
            return false;
        }

        var deleteRequest = new DeleteObjectRequest { BucketName = _configuration.Bucket, Key = GetKey(path) };
        await s3.DeleteObjectAsync(deleteRequest);

        return true;
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
        };
        if (!string.IsNullOrWhiteSpace(_configuration.Server))
            s3Cfg.ServiceURL = _configuration.Server;

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

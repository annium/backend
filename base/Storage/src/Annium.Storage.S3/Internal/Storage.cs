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

internal class Storage : IStorage, ILogSubject
{
    public ILogger Logger { get; }
    private readonly Configuration _configuration;
    private readonly string _directory;

    public Storage(Configuration configuration, ILogger logger)
    {
        VerifyRoot(configuration.Directory);

        _configuration = configuration;
        _directory = configuration.Directory == "/" ? string.Empty : configuration.Directory.TrimStart('/');
        Logger = logger;
    }

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

    private string GetKey(string name)
    {
        return _directory == string.Empty ? name : Path.Combine(_directory, name);
    }
}

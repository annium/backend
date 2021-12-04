using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;

namespace Annium.Storage.S3;

internal class Storage : StorageBase
{
    private readonly Configuration _configuration;
    private readonly string _directory;

    public Storage(
        Configuration configuration,
        ILogger<Storage> logger
    ) : base(logger)
    {
        _configuration = configuration ??
                         throw new ArgumentNullException(nameof(configuration));

        VerifyPath(configuration.Directory);

        _directory = configuration.Directory == "/" ? string.Empty : configuration.Directory.TrimStart('/');
    }

    protected override async Task DoSetupAsync()
    {
        using (var s3 = GetClient())
        {
            var buckets = (await s3.ListBucketsAsync()).Buckets.Select(b => b.BucketName).ToArray();
            if (buckets.Contains(_configuration.Bucket))
                return;

            await s3.PutBucketAsync(new PutBucketRequest { BucketName = _configuration.Bucket });
        }
    }

    protected override async Task<string[]> DoListAsync()
    {
        var listRequest = new ListObjectsRequest { BucketName = _configuration.Bucket, MaxKeys = 100, Prefix = _directory };

        using (var s3 = GetClient())
        {
            var objects = (await s3.ListObjectsAsync(listRequest)).S3Objects;

            return objects
                .Select(o => ReadKey(o.Key))
                .ToArray();
        }
    }

    protected override async Task DoUploadAsync(Stream source, string name)
    {
        VerifyName(name);

        source.Position = 0;
        var putRequest = new PutObjectRequest { BucketName = _configuration.Bucket, Key = GetKey(name), InputStream = source, };

        using (var s3 = GetClient())
        {
            await s3.PutObjectAsync(putRequest);
        }
    }

    protected override async Task<Stream> DoDownloadAsync(string name)
    {
        VerifyName(name);

        using (var s3 = GetClient())
        {
            try
            {
                var getRequest = new GetObjectRequest { BucketName = _configuration.Bucket, Key = GetKey(name) };
                using (var getResponse = await s3.GetObjectAsync(getRequest))
                {
                    var ms = new MemoryStream();
                    await getResponse.ResponseStream.CopyToAsync(ms);
                    ms.Position = 0;

                    return ms;
                }
            }
            catch (AmazonS3Exception)
            {
                // assume object was not found
                throw new KeyNotFoundException($"{name} not found in storage");
            }
        }
    }

    protected override async Task<bool> DoDeleteAsync(string name)
    {
        VerifyName(name);

        using (var s3 = GetClient())
        {
            var getRequest = new GetObjectRequest { BucketName = _configuration.Bucket, Key = GetKey(name) };
            try
            {
                await s3.GetObjectAsync(getRequest);
            }
            catch (AmazonS3Exception)
            {
                // assume object was not found
                return false;
            }

            var deleteRequest = new DeleteObjectRequest { BucketName = _configuration.Bucket, Key = GetKey(name) };
            var deleteResponse = await s3.DeleteObjectAsync(deleteRequest);

            return true;
        }
    }

    private IAmazonS3 GetClient()
    {
        if (string.IsNullOrWhiteSpace(_configuration.AccessKey))
            throw new ArgumentException("Access key is required");

        if (string.IsNullOrWhiteSpace(_configuration.AccessSecret))
            throw new ArgumentException("Access secret is required");

        if (string.IsNullOrWhiteSpace(_configuration.Bucket))
            throw new ArgumentException("Bucket name is required");

        var s3Cfg = new AmazonS3Config();
        s3Cfg.RegionEndpoint = RegionEndpoint.GetBySystemName(_configuration.Region);
        if (!string.IsNullOrWhiteSpace(_configuration.Server))
            s3Cfg.ServiceURL = _configuration.Server;

        return new AmazonS3Client(_configuration.AccessKey, _configuration.AccessSecret, s3Cfg);
    }

    private string GetKey(string name) =>
        _directory == string.Empty ? name : $"{_directory}/{name}";

    private string ReadKey(string key) =>
        Path.GetFileName(key);
}
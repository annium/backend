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

namespace Annium.Storage.S3
{
    internal class Storage : StorageBase
    {
        private readonly Configuration configuration;
        private readonly string directory;

        public Storage(
            Configuration configuration,
            ILogger<Storage> logger
        ) : base(logger)
        {
            this.configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));

            VerifyPath(configuration.Directory);

            directory = configuration.Directory == "/" ?
                string.Empty :
                configuration.Directory.TrimStart('/');
        }

        protected override async Task DoSetupAsync()
        {
            using(var s3 = GetClient())
            {
                var buckets = (await s3.ListBucketsAsync()).Buckets.Select(b => b.BucketName).ToArray();
                if (buckets.Contains(configuration.Bucket))
                    return;

                await s3.PutBucketAsync(new PutBucketRequest { BucketName = configuration.Bucket });
            }
        }

        protected override async Task<string[]> DoListAsync()
        {
            var listRequest = new ListObjectsRequest() { BucketName = configuration.Bucket, MaxKeys = 100, Prefix = directory };

            using(var s3 = GetClient())
            {
                var objects = (await s3.ListObjectsAsync(listRequest)).S3Objects;

                return objects
                    .Select(o => readKey(o.Key))
                    .ToArray();
            }
        }

        protected override async Task DoUploadAsync(Stream source, string name)
        {
            VerifyName(name);

            source.Position = 0;
            var putRequest = new PutObjectRequest() { BucketName = configuration.Bucket, Key = getKey(name), InputStream = source, };

            using(var s3 = GetClient())
            {
                await s3.PutObjectAsync(putRequest);
            }
        }

        protected override async Task<Stream> DoDownloadAsync(string name)
        {
            VerifyName(name);

            using(var s3 = GetClient())
            {
                try
                {
                    var getRequest = new GetObjectRequest() { BucketName = configuration.Bucket, Key = getKey(name) };
                    using(var getResponse = await s3.GetObjectAsync(getRequest))
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

            using(var s3 = GetClient())
            {
                var getRequest = new GetObjectRequest() { BucketName = configuration.Bucket, Key = getKey(name) };
                try
                {
                    await s3.GetObjectAsync(getRequest);
                }
                catch (AmazonS3Exception)
                {
                    // assume object was not found
                    return false;
                }

                var deleteRequest = new DeleteObjectRequest() { BucketName = configuration.Bucket, Key = getKey(name) };
                var deleteResponse = await s3.DeleteObjectAsync(deleteRequest);

                return true;
            }
        }

        private IAmazonS3 GetClient()
        {
            if (string.IsNullOrWhiteSpace(configuration.AccessKey))
                throw new ArgumentException("Access key is required");

            if (string.IsNullOrWhiteSpace(configuration.AccessSecret))
                throw new ArgumentException("Access secret is required");

            if (string.IsNullOrWhiteSpace(configuration.Bucket))
                throw new ArgumentException("Bucket name is required");

            var s3cfg = new AmazonS3Config();
            s3cfg.RegionEndpoint = RegionEndpoint.GetBySystemName(configuration.Region);
            if (!string.IsNullOrWhiteSpace(configuration.Server))
                s3cfg.ServiceURL = configuration.Server;

            return new AmazonS3Client(configuration.AccessKey, configuration.AccessSecret, s3cfg);
        }

        private string getKey(string name) =>
            directory == string.Empty ? name : $"{directory}/{name}";

        private string readKey(string key) =>
            Path.GetFileName(key);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Storage.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Storage.FileSystem.Tests
{
    public class StorageTest : IDisposable
    {
        private readonly string _directory;

        public StorageTest()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        [Fact]
        public async Task Setup_Works()
        {
            // arrange
            var storage = await GetStorage();

            // act
            await storage.SetupAsync();
        }

        [Fact]
        public async Task List_Works()
        {
            // arrange
            var storage = await GetStorage();
            var blob = GenerateBlob();
            await storage.UploadAsync(new MemoryStream(blob), "demo");

            // act
            var keys = await storage.ListAsync();

            // assert
            keys.Has(1);
            keys.At(0).IsEqual("demo");
        }

        [Fact]
        public async Task Upload_Works()
        {
            // arrange
            var storage = await GetStorage();
            var blob = GenerateBlob();

            // act
            await storage.UploadAsync(new MemoryStream(blob), "demo");
            var keys = await storage.ListAsync();

            // assert
            keys.Has(1);
            keys.At(0).IsEqual("demo");
        }

        [Fact]
        public async Task Download_Missing_ThrowsKeyNotFoundException()
        {
            // arrange
            var storage = await GetStorage();

            // act
            var e = new Exception();
            try
            {
                await storage.DownloadAsync("demo");
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.As<KeyNotFoundException>();
        }

        [Fact]
        public async Task Download_Works()
        {
            // arrange
            var storage = await GetStorage();
            var blob = GenerateBlob();
            await storage.UploadAsync(new MemoryStream(blob), "demo");

            // act
            byte[] result;
            using (var ms = new MemoryStream())
            {
                await (await storage.DownloadAsync("demo")).CopyToAsync(ms);
                result = ms.ToArray();
            }

            // assert
            ((Span<byte>) result).SequenceEqual((Span<byte>) blob).IsTrue();
        }

        [Fact]
        public async Task NameVerification_Works()
        {
            // arrange
            var storage = await GetStorage();

            // assert
            var e = new Exception();
            try
            {
                await storage.DownloadAsync(".");
            }
            catch (Exception ex)
            {
                e = ex;
            }

            e.As<ArgumentException>();
        }

        [Fact]
        public async Task Delete_Works()
        {
            // arrange
            var storage = await GetStorage();
            var blob = GenerateBlob();
            await storage.UploadAsync(new MemoryStream(blob), "demo");

            // act
            var first = await storage.DeleteAsync("demo");
            var second = await storage.DeleteAsync("demo");

            // assert
            first.IsTrue();
            second.IsFalse();
        }

        private async Task<IStorage> GetStorage()
        {
            var container = new ServiceContainer();
            container.AddStorage().AddFileSystemStorage();
            container.AddLogging(route => route.UseInMemory());
            container.AddManagedTimeProvider();

            var provider = container.BuildServiceProvider();

            var factory = provider.Resolve<IStorageFactory>();
            var configuration = new Configuration();
            configuration.Directory = _directory;

            var storage = factory.CreateStorage(configuration);
            await storage.SetupAsync();

            return storage;
        }

        private byte[] GenerateBlob() => Encoding.UTF8.GetBytes("sample text file");

        public void Dispose()
        {
            Directory.Delete(_directory, true);
        }
    }
}
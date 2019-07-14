using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Annium.Extensions.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Storage.InMemory.Tests
{
    public class StorageTest
    {
        private readonly Random random = new Random();

        [Fact]
        public async Task Setup_Works()
        {
            // arrange
            var storage = GetStorage();

            // act
            await storage.SetupAsync();
        }

        [Fact]
        public async Task List_Works()
        {
            // arrange
            var storage = GetStorage();
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
            var storage = GetStorage();
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
            var storage = GetStorage();

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
            e.Is<KeyNotFoundException>();
        }

        [Fact]
        public async Task Download_Works()
        {
            // arrange
            var storage = GetStorage();
            var blob = GenerateBlob();
            await storage.UploadAsync(new MemoryStream(blob), "demo");

            // act
            byte[] result;
            using(var ms = new MemoryStream())
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
            var storage = GetStorage();

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
            e.Is<ArgumentException>();
        }

        [Fact]
        public async Task Delete_Works()
        {
            // arrange
            var storage = GetStorage();
            var blob = GenerateBlob();
            await storage.UploadAsync(new MemoryStream(blob), "demo");

            // act
            var first = await storage.DeleteAsync("demo");
            var second = await storage.DeleteAsync("demo");

            // assert
            first.IsTrue();
            second.IsFalse();
        }

        private StorageBase GetStorage()
        {
            var services = new ServiceCollection();
            services.AddStorage().AddInMemoryStorage();
            services.AddInMemoryLogger(new LoggerConfiguration(LogLevel.Trace));
            services.AddSingleton<Func<Instant>>(() => Instant.MinValue);

            var provider = services.BuildServiceProvider();

            var factory = provider.GetRequiredService<IStorageFactory>();

            return factory.CreateStorage(new Configuration());
        }

        private byte[] GenerateBlob() => Encoding.UTF8.GetBytes("sample text file");
    }
}
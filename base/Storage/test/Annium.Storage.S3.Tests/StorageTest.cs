using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Storage.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Storage.S3.Tests;

public class StorageTest : IAsyncDisposable
{
    private readonly string _directory;
    private const string FileName = "demo.txt";

    public StorageTest()
    {
        _directory = $"/storage_test/{Guid.NewGuid().ToString()}";
    }

    [Fact(Skip = "Needs durable test basis")]
    public async Task Setup_Works()
    {
        // arrange
        var storage = await GetStorage();

        // act
        await storage.SetupAsync();
    }

    [Fact(Skip = "Needs durable test basis")]
    public async Task List_Works()
    {
        // arrange
        var storage = await GetStorage();
        var blob = GenerateBlob();
        await storage.UploadAsync(new MemoryStream(blob), FileName);

        // act
        var keys = await storage.ListAsync();

        // assert
        keys.Has(1);
        keys.At(0).Is(FileName);
    }

    [Fact(Skip = "Needs durable test basis")]
    public async Task Upload_Works()
    {
        // arrange
        var storage = await GetStorage();
        var blob = GenerateBlob();

        // act
        await storage.UploadAsync(new MemoryStream(blob), FileName);
        var keys = await storage.ListAsync();

        // assert
        keys.Has(1);
        keys.At(0).Is(FileName);
    }

    [Fact(Skip = "Needs durable test basis")]
    public async Task Download_Missing_ThrowsKeyNotFoundException()
    {
        // arrange
        var storage = await GetStorage();

        // act
        var e = new Exception();
        try
        {
            await storage.DownloadAsync(FileName);
        }
        catch (Exception ex)
        {
            e = ex;
        }

        e.As<KeyNotFoundException>();
    }

    [Fact(Skip = "Needs durable test basis")]
    public async Task Download_Works()
    {
        // arrange
        var storage = await GetStorage();
        var blob = GenerateBlob();
        await storage.UploadAsync(new MemoryStream(blob), FileName);

        // act
        byte[] result;
        using (var ms = new MemoryStream())
        {
            await (await storage.DownloadAsync(FileName)).CopyToAsync(ms);
            result = ms.ToArray();
        }

        // assert
        ((Span<byte>) result).SequenceEqual((Span<byte>) blob).IsTrue();
    }

    [Fact(Skip = "Needs durable test basis")]
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

    [Fact(Skip = "Needs durable test basis")]
    public async Task Delete_Works()
    {
        // arrange
        var storage = await GetStorage();
        var blob = GenerateBlob();
        await storage.UploadAsync(new MemoryStream(blob), FileName);

        // act
        var first = await storage.DeleteAsync(FileName);
        var second = await storage.DeleteAsync(FileName);

        // assert
        first.IsTrue();
        second.IsFalse();
    }

    private async Task<IStorage> GetStorage()
    {
        var container = new ServiceContainer();
        container.AddStorage().AddS3Storage();
        container.AddLogging();
        container.AddTime().WithManagedTime().SetDefault();

        var provider = container.BuildServiceProvider();

        provider.UseLogging(route => route.UseInMemory());

        var factory = provider.Resolve<IStorageFactory>();
        var configuration = new Configuration();
        configuration.Server = "https://s3.yandexcloud.net";
        configuration.AccessKey = "YCAJEoZt5_iy39ldI0y62gdME";
        configuration.AccessSecret = "YCMlxyUBNX37JFy9_0h5_kqXcDH-BO6kTKdznQ-n";
        configuration.Region = "us-east-1";
        configuration.Bucket = "annium.tests";
        configuration.Directory = _directory;

        var storage = factory.CreateStorage(configuration);
        await storage.SetupAsync();

        return storage;
    }

    private byte[] GenerateBlob() => "sample text file"u8.ToArray();

    public async ValueTask DisposeAsync()
    {
        var storage = await GetStorage();
        foreach (var item in await storage.ListAsync())
            await storage.DeleteAsync(item);
    }
}
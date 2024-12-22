using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Storage.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Storage.Base.Tests;

public abstract class StorageTestBase
{
    [Fact]
    public async Task List_Works()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();
        await storage.UploadAsync(new MemoryStream(blob), "list_test");

        // act
        var keys = await storage.ListAsync();

        // assert
        keys.Contains("list_test").IsTrue();
    }

    [Fact]
    public async Task List_Prefixed_Works()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();
        await storage.UploadAsync(new MemoryStream(blob), "list_prefixed_one/a");
        await storage.UploadAsync(new MemoryStream(blob), "list_prefixed_one/b");
        await storage.UploadAsync(new MemoryStream(blob), "list_prefixed_two/a");

        // act
        var keysOne = await storage.ListAsync("list_prefixed_one");
        var keysTwo = await storage.ListAsync("list_prefixed_two");

        // assert
        keysOne.Has(2);
        keysOne.Contains("list_prefixed_one/a").IsTrue();
        keysOne.Contains("list_prefixed_one/b").IsTrue();
        keysTwo.Has(1);
        keysTwo.Contains("list_prefixed_two/a").IsTrue();
    }

    [Fact]
    public async Task Upload_Works()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();

        // act
        await storage.UploadAsync(new MemoryStream(blob), "upload_test");
        var keys = await storage.ListAsync();

        // assert
        keys.Contains("upload_test").IsTrue();
    }

    [Fact]
    public async Task Download_Missing_ThrowsKeyNotFoundException()
    {
        // arrange
        var storage = GetStorage();

        // act
        await Wrap.It(async () => await storage.DownloadAsync("download_missing")).ThrowsAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Download_Works()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();
        await storage.UploadAsync(new MemoryStream(blob), "download_test");

        // act
        byte[] result;

        using (var ms = new MemoryStream())
        {
            await (await storage.DownloadAsync("download_test")).CopyToAsync(ms);
            result = ms.ToArray();
        }

        // assert
        result.SequenceEqual(blob).IsTrue();
    }

    [Fact]
    public async Task NameVerification_Works()
    {
        // arrange
        var storage = GetStorage();

        // assert
        await Wrap.It(async () => await storage.DownloadAsync(".")).ThrowsAsync<ArgumentException>();
    }

    [Fact]
    public async Task Delete_Works()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();
        await storage.DeleteAsync("delete_test");
        await storage.UploadAsync(new MemoryStream(blob), "delete_test");

        // act
        var first = await storage.DeleteAsync("delete_test");
        var second = await storage.DeleteAsync("delete_test");

        // assert
        first.IsTrue();
        second.IsFalse();
    }

    protected abstract IStorage GetStorage();

    private static byte[] GenerateBlob()
    {
        return "sample text file"u8.ToArray();
    }
}

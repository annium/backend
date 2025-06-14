using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Storage.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Storage.Tests.Lib;

/// <summary>
/// Abstract base class containing common test scenarios for storage implementations.
/// Provides a standardized test suite that can be inherited by concrete storage provider tests.
/// </summary>
public abstract class StorageTestBase
{
    /// <summary>
    /// Tests that the List operation returns all stored items.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that the List operation with prefix filtering returns only items matching the specified prefix.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that the Upload operation successfully stores data and makes it listable.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Tests that attempting to download a non-existent item throws a KeyNotFoundException.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Download_Missing_ThrowsKeyNotFoundException()
    {
        // arrange
        var storage = GetStorage();

        // act
        await Wrap.It(async () => await storage.DownloadAsync("download_missing")).ThrowsAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Tests that the Download operation successfully retrieves previously uploaded data.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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
            await (await storage.DownloadAsync("download_test")).CopyToAsync(ms, TestContext.Current.CancellationToken);
            result = ms.ToArray();
        }

        // assert
        result.SequenceEqual(blob).IsTrue();
    }

    /// <summary>
    /// Tests that invalid item names are properly validated and rejected.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task NameVerification_Works()
    {
        // arrange
        var storage = GetStorage();

        // assert
        await Wrap.It(async () => await storage.DownloadAsync(".")).ThrowsAsync<ArgumentException>();
    }

    /// <summary>
    /// Tests that the Delete operation removes items and returns appropriate status indicators.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Creates and configures a storage instance for testing.
    /// Must be implemented by concrete test classes to provide their specific storage implementation.
    /// </summary>
    /// <returns>A configured storage instance for testing.</returns>
    protected abstract IStorage GetStorage();

    /// <summary>
    /// Generates a sample byte array for testing storage operations.
    /// </summary>
    /// <returns>A byte array containing test data.</returns>
    private static byte[] GenerateBlob()
    {
        return "sample text file"u8.ToArray();
    }
}

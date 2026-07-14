using System;
using System.Collections.Generic;
using System.IO;
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
    /// Tests that uploading over an item that is already stored replaces it, rather than keeping
    /// what was there or storing both.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Upload_ExistingKey_OverwritesValue()
    {
        // arrange
        var storage = GetStorage();
        await storage.UploadAsync(new MemoryStream(GenerateBlob()), "upload_existing");

        // act: shorter than what it replaces, so a write that fails to truncate leaves a tail behind
        var blob = "brief"u8.ToArray();
        await storage.UploadAsync(new MemoryStream(blob), "upload_existing");

        // assert
        byte[] result;
        using (var ms = new MemoryStream())
        {
            await (await storage.DownloadAsync("upload_existing")).CopyToAsync(
                ms,
                TestContext.Current.CancellationToken
            );
            result = ms.ToArray();
        }

        result.SequenceEqual(blob).IsTrue();
        (await storage.ListAsync("upload_existing")).Has(1);
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
    /// Tests that a prefix naming a stored item exactly matches that item, rather than only
    /// matching items nested beneath it.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task List_ExactPrefix_MatchesItem()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();
        await storage.UploadAsync(new MemoryStream(blob), "list_exact");

        // act
        var keys = await storage.ListAsync("list_exact");

        // assert
        keys.Has(1);
        keys.Contains("list_exact").IsTrue();
    }

    /// <summary>
    /// Tests that uploading stores the whole stream, not only the part after wherever the caller
    /// happened to leave its position.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Upload_ReadSource_StoresWholeStream()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();
        var source = new MemoryStream(blob);
        source.ReadByte();

        // act
        await storage.UploadAsync(source, "upload_read_source");
        byte[] result;
        using (var ms = new MemoryStream())
        {
            await (await storage.DownloadAsync("upload_read_source")).CopyToAsync(
                ms,
                TestContext.Current.CancellationToken
            );
            result = ms.ToArray();
        }

        // assert
        result.SequenceEqual(blob).IsTrue();
    }

    /// <summary>
    /// Tests that a prefix matches whole path segments, so an item merely starting with the same
    /// characters is not treated as being under it.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task List_SimilarPrefix_NotMatched()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();
        await storage.UploadAsync(new MemoryStream(blob), "list_boundary/a");
        await storage.UploadAsync(new MemoryStream(blob), "list_boundary_other/a");

        // act
        var keys = await storage.ListAsync("list_boundary");

        // assert
        keys.Has(1);
        keys.Contains("list_boundary/a").IsTrue();
    }

    /// <summary>
    /// Tests that listing a prefix nothing was stored under returns an empty result rather than failing.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task List_NoMatches_ReturnsEmpty()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();
        await storage.UploadAsync(new MemoryStream(blob), "list_nomatches/a");

        // act
        var keys = await storage.ListAsync("list_nomatches_other");

        // assert
        keys.IsEmpty();
    }

    /// <summary>
    /// Tests that invalid item names are properly validated and rejected by every operation,
    /// not just by a single representative one.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task NameVerification_Works()
    {
        // arrange
        var storage = GetStorage();
        var blob = GenerateBlob();

        // assert
        await Wrap.It(async () => await storage.DownloadAsync(".")).ThrowsAsync<ArgumentException>();
        await Wrap.It(async () => await storage.UploadAsync(new MemoryStream(blob), "."))
            .ThrowsAsync<ArgumentException>();
        await Wrap.It(async () => await storage.DeleteAsync(".")).ThrowsAsync<ArgumentException>();
        await Wrap.It(async () => await storage.ListAsync("/absolute")).ThrowsAsync<ArgumentException>();
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

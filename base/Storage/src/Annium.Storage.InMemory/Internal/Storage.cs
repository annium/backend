using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Storage.Abstractions;
using static Annium.Storage.Abstractions.StorageHelper;

namespace Annium.Storage.InMemory.Internal;

/// <summary>
/// In-memory implementation of the IStorage interface that stores files in memory using a concurrent dictionary
/// </summary>
internal class Storage(ILogger logger) : IStorage, ILogSubject
{
    /// <summary>
    /// Logger instance for this storage implementation
    /// </summary>
    public ILogger Logger { get; } = logger;

    /// <summary>
    /// Thread-safe dictionary that stores file content as byte arrays in memory
    /// </summary>
    private readonly ConcurrentDictionary<string, byte[]> _storage = new();

    /// <summary>
    /// Lists all files in the storage with an optional prefix filter
    /// </summary>
    /// <param name="prefix">Optional prefix to filter files. Empty string returns all files</param>
    /// <returns>Array of file paths matching the prefix</returns>
    public Task<string[]> ListAsync(string prefix = "")
    {
        VerifyPrefix(prefix);

        if (prefix == "")
            return Task.FromResult(_storage.Keys.ToArray());

        var keys = _storage.Where(x => x.Key == prefix || x.Key.StartsWith($"{prefix}/")).Select(x => x.Key).ToArray();

        return Task.FromResult(keys);
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

        using var ms = new MemoryStream();
        ms.Position = 0;
        await source.CopyToAsync(ms);

        _storage[path] = ms.ToArray();
    }

    /// <summary>
    /// Downloads a file from storage as a stream
    /// </summary>
    /// <param name="path">The path of the file to download</param>
    /// <returns>A stream containing the file content</returns>
    public Task<Stream> DownloadAsync(string path)
    {
        VerifyPath(path);

        if (!_storage.TryGetValue(path, out var value))
            throw new KeyNotFoundException($"{path} not found in storage");

        return Task.FromResult<Stream>(new MemoryStream(value));
    }

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="path">The path of the file to delete</param>
    /// <returns>True if the file was deleted, false if it did not exist</returns>
    public Task<bool> DeleteAsync(string path)
    {
        VerifyPath(path);

        return Task.FromResult(_storage.Remove(path, out _));
    }
}

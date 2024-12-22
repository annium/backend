using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Storage.Abstractions;
using static Annium.Storage.Abstractions.StorageHelper;

namespace Annium.Storage.InMemory.Internal;

internal class Storage(ILogger logger) : IStorage, ILogSubject
{
    public ILogger Logger { get; } = logger;

    private readonly ConcurrentDictionary<string, byte[]> _storage = new();

    public Task<string[]> ListAsync(string prefix = "")
    {
        VerifyPrefix(prefix);

        if (prefix == "")
            return Task.FromResult(_storage.Keys.ToArray());

        var keys = _storage.Where(x => x.Key == prefix || x.Key.StartsWith($"{prefix}/")).Select(x => x.Key).ToArray();

        return Task.FromResult(keys);
    }

    public async Task UploadAsync(Stream source, string path)
    {
        VerifyPath(path);

        using var ms = new MemoryStream();
        ms.Position = 0;
        await source.CopyToAsync(ms);

        _storage[path] = ms.ToArray();
    }

    public Task<Stream> DownloadAsync(string path)
    {
        VerifyPath(path);

        if (!_storage.TryGetValue(path, out var value))
            throw new KeyNotFoundException($"{path} not found in storage");

        return Task.FromResult<Stream>(new MemoryStream(value));
    }

    public Task<bool> DeleteAsync(string path)
    {
        VerifyPath(path);

        return Task.FromResult(_storage.Remove(path, out _));
    }
}

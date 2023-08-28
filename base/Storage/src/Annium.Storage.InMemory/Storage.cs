using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Storage.Abstractions;

namespace Annium.Storage.InMemory;

internal class Storage : StorageBase
{
    private readonly IDictionary<string, byte[]> _storage = new Dictionary<string, byte[]>();

    public Storage(
        ILogger logger
    ) : base(logger)
    {
    }

    protected override Task DoSetupAsync() => Task.CompletedTask;

    protected override Task<string[]> DoListAsync() => Task.FromResult(_storage.Keys.ToArray());

    protected override async Task DoUploadAsync(Stream source, string name)
    {
        VerifyName(name);

        using var ms = new MemoryStream();
        ms.Position = 0;
        await source.CopyToAsync(ms);
        _storage[name] = ms.ToArray();
    }

    protected override Task<Stream> DoDownloadAsync(string name)
    {
        VerifyName(name);

        if (!_storage.ContainsKey(name))
            throw new KeyNotFoundException($"{name} not found in storage");

        return Task.FromResult<Stream>(new MemoryStream(_storage[name]));
    }

    protected override Task<bool> DoDeleteAsync(string name)
    {
        VerifyName(name);

        if (!_storage.ContainsKey(name))
            return Task.FromResult(false);

        _storage.Remove(name);

        return Task.FromResult(true);
    }
}
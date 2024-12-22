using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Storage.Abstractions;
using static Annium.Storage.Abstractions.StorageHelper;

namespace Annium.Storage.FileSystem.Internal;

internal class Storage : IStorage, ILogSubject
{
    public ILogger Logger { get; }

    private readonly string _directory;

    public Storage(Configuration configuration, ILogger logger)
    {
        VerifyRoot(configuration.Directory);

        _directory = configuration.Directory;
        Logger = logger;
    }

    public Task<string[]> ListAsync(string prefix = "")
    {
        var root = prefix == "" ? _directory : Path.Combine(_directory, prefix);
        var files = Directory
            .GetFiles(root, "*", SearchOption.AllDirectories)
            .Select(e => Path.GetRelativePath(_directory, e))
            .ToArray();

        return Task.FromResult(files);
    }

    public async Task UploadAsync(Stream source, string path)
    {
        VerifyPath(path);

        var fullPath = Path.Combine(_directory, path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath).NotNull());
        await using var target = File.Open(fullPath, FileMode.Create);
        source.Position = 0;
        await source.CopyToAsync(target);
    }

    public async Task<Stream> DownloadAsync(string path)
    {
        VerifyPath(path);

        var fullPath = Path.Combine(_directory, path);
        if (!File.Exists(fullPath))
            throw new KeyNotFoundException($"{path} not found in storage");

        await using var source = File.Open(fullPath, FileMode.Open);
        var ms = new MemoryStream();

        source.Position = 0;
        await source.CopyToAsync(ms);
        ms.Position = 0;

        return ms;
    }

    public Task<bool> DeleteAsync(string path)
    {
        VerifyPath(path);

        var fullPath = Path.Combine(_directory, path);
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);

        return Task.FromResult(true);
    }
}

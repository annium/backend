using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Storage.Abstractions;
using static Annium.Storage.Abstractions.StorageHelper;

namespace Annium.Storage.FileSystem.Internal;

/// <summary>
/// File system implementation of the IStorage interface that stores files on the local file system
/// </summary>
internal class Storage : IStorage, ILogSubject
{
    /// <summary>
    /// Logger instance for this storage implementation
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The root directory where files are stored
    /// </summary>
    private readonly string _directory;

    /// <summary>
    /// Initializes a new instance of the file system storage with the specified configuration
    /// </summary>
    /// <param name="configuration">The configuration containing the root directory path</param>
    /// <param name="logger">The logger instance for logging operations</param>
    public Storage(Configuration configuration, ILogger logger)
    {
        VerifyRoot(configuration.Directory);

        _directory = configuration.Directory;
        Logger = logger;
    }

    /// <summary>
    /// Lists all files in the storage with an optional prefix filter
    /// </summary>
    /// <param name="prefix">Optional prefix to filter files. Empty string returns all files</param>
    /// <returns>Array of file paths matching the prefix</returns>
    public Task<string[]> ListAsync(string prefix = "")
    {
        var root = prefix == "" ? _directory : Path.Combine(_directory, prefix);
        var files = Directory
            .GetFiles(root, "*", SearchOption.AllDirectories)
            .Select(e => Path.GetRelativePath(_directory, e))
            .ToArray();

        return Task.FromResult(files);
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

        var fullPath = Path.Combine(_directory, path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath).NotNull());
        await using var target = File.Open(fullPath, FileMode.Create);
        source.Position = 0;
        await source.CopyToAsync(target);
    }

    /// <summary>
    /// Downloads a file from storage as a stream
    /// </summary>
    /// <param name="path">The path of the file to download</param>
    /// <returns>A stream containing the file content</returns>
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

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="path">The path of the file to delete</param>
    /// <returns>True if the file was deleted, false if it did not exist</returns>
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

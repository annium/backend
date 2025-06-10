using System.IO;
using System.Threading.Tasks;

namespace Annium.Storage.Abstractions;

/// <summary>
/// Provides an abstraction for storage operations allowing listing, uploading, downloading, and deleting files
/// </summary>
public interface IStorage
{
    /// <summary>
    /// Lists all files in the storage with an optional prefix filter
    /// </summary>
    /// <param name="prefix">Optional prefix to filter files. Empty string returns all files</param>
    /// <returns>Array of file paths matching the prefix</returns>
    Task<string[]> ListAsync(string prefix = "");

    /// <summary>
    /// Uploads a stream to the specified path in storage
    /// </summary>
    /// <param name="source">The stream containing the data to upload</param>
    /// <param name="path">The destination path in storage</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    Task UploadAsync(Stream source, string path);

    /// <summary>
    /// Downloads a file from storage as a stream
    /// </summary>
    /// <param name="path">The path of the file to download</param>
    /// <returns>A stream containing the file content</returns>
    Task<Stream> DownloadAsync(string path);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="path">The path of the file to delete</param>
    /// <returns>True if the file was deleted, false if it did not exist</returns>
    Task<bool> DeleteAsync(string path);
}

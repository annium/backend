using System.IO;
using System.Threading.Tasks;

namespace Annium.Storage.Abstractions;

public interface IStorage
{
    Task SetupAsync();

    Task<string[]> ListAsync();

    Task UploadAsync(Stream source, string name);

    Task<Stream> DownloadAsync(string name);

    Task<bool> DeleteAsync(string name);
}
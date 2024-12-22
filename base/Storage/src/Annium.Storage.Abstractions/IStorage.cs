using System.IO;
using System.Threading.Tasks;

namespace Annium.Storage.Abstractions;

public interface IStorage
{
    Task<string[]> ListAsync(string prefix = "");
    Task UploadAsync(Stream source, string path);
    Task<Stream> DownloadAsync(string path);
    Task<bool> DeleteAsync(string path);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace Annium.Redis;

public interface IRedisStorage
{
    Task<IReadOnlyCollection<string>> GetKeysAsync(string pattern = "");
    Task<string?> GetAsync(string key);
    Task<bool> SetAsync(string key, string value, Duration expires = default);
    Task<bool> DeleteAsync(string key);
}

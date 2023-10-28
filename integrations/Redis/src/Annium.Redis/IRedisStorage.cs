using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace Annium.Redis;

public interface IRedisStorage
{
    Task<IReadOnlyCollection<string>> GetKeys(string pattern = "");
    Task<string?> Get(string key);
    Task<bool> Set(string key, string value, Duration expires = default);
    Task<bool> Delete(string key);
}

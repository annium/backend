using System.Collections.Generic;

namespace Annium.Extensions.Configuration
{
    public interface IConfigurationBuilder
    {
        IConfigurationBuilder Add(IReadOnlyDictionary<string, string> config);

        T Build<T>()
        where T : class, new();
    }
}
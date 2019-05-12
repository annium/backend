using System.Collections.Generic;

namespace Annium.Extensions.Configuration
{
    internal interface IConfigurationProvider
    {
        IReadOnlyDictionary<string[], string> Read();
    }
}
using Annium.Core.Runtime;
using Annium.Core.Runtime.Types;

namespace Annium.Storage.Abstractions
{
    public abstract class ConfigurationBase
    {
        [ResolveField]
        public string Type { get; set; } = string.Empty;
    }
}
using Annium.Core.Reflection;

namespace Annium.Storage.Abstractions
{
    public abstract class ConfigurationBase
    {
        [ResolveField]
        public string Type { get; set; }
    }
}
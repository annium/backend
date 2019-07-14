using Annium.Core.Application.Types;

namespace Annium.Storage.Abstractions
{
    public abstract class ConfigurationBase
    {
        [ResolveField]
        public string Type { get; set; }
    }
}
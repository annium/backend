using Annium.Core.Runtime.Types;

namespace Annium.Storage.Abstractions;

public abstract class ConfigurationBase
{
    [ResolutionKey]
    public string Type { get; set; } = string.Empty;
}

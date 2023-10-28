using Annium.Core.Runtime.Types;
using Annium.Storage.Abstractions;

namespace Annium.Storage.FileSystem;

[ResolutionKeyValue("fs")]
public class Configuration : ConfigurationBase
{
    public string Directory { get; set; } = string.Empty;
}

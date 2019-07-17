using Annium.Core.Application.Types;

namespace Annium.Storage.FileSystem
{
    [ResolveKey("fs")]
    public class Configuration : Abstractions.ConfigurationBase
    {
        public string Directory { get; set; }
    }
}
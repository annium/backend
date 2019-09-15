using Annium.Core.Reflection;

namespace Annium.Storage.FileSystem
{
    [ResolveKey("fs")]
    public class Configuration : Abstractions.ConfigurationBase
    {
        public string Directory { get; set; }
    }
}
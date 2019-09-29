using Annium.Core.Reflection;

namespace Annium.Storage.S3
{
    [ResolveKey("s3")]
    public class Configuration : Abstractions.ConfigurationBase
    {
        public string Server { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public string AccessSecret { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public string Bucket { get; set; } = string.Empty;

        public string Directory { get; set; } = string.Empty;
    }
}
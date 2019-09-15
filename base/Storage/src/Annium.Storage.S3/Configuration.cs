using Annium.Core.Reflection;

namespace Annium.Storage.S3
{
    [ResolveKey("s3")]
    public class Configuration : Abstractions.ConfigurationBase
    {
        public string Server { get; set; }

        public string AccessKey { get; set; }

        public string AccessSecret { get; set; }

        public string Region { get; set; }

        public string Bucket { get; set; }

        public string Directory { get; set; }
    }
}
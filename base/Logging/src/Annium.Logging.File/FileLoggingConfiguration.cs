using Annium.Logging.Shared;

namespace Annium.Logging.File
{
    public class FileLoggingConfiguration : LogRouteConfiguration
    {
        public string File { get; set; } = string.Empty;
    }
}
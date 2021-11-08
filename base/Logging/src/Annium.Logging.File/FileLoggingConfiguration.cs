using Annium.Logging.Shared;

namespace Annium.Logging.File
{
    public record FileLoggingConfiguration : LogRouteConfiguration
    {
        public string File { get; set; } = string.Empty;
    }
}
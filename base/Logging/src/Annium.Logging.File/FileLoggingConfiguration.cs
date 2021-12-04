using Annium.Logging.Shared;

namespace Annium.Logging.File;

public record FileLoggingConfiguration : LogRouteConfiguration
{
    public string File { get; init; } = string.Empty;
}
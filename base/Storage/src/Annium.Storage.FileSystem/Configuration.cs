namespace Annium.Storage.FileSystem;

public record Configuration
{
    public string Directory { get; init; } = string.Empty;
}

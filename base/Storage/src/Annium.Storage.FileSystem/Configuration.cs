namespace Annium.Storage.FileSystem;

/// <summary>
/// Configuration settings for file system storage implementation
/// </summary>
public record Configuration
{
    /// <summary>
    /// The root directory path where files will be stored
    /// </summary>
    public string Directory { get; init; } = string.Empty;
}

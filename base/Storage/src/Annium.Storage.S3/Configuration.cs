namespace Annium.Storage.S3;

public record Configuration
{
    public string Server { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string AccessSecret { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string Bucket { get; init; } = string.Empty;
    public string Directory { get; init; } = string.Empty;
}

namespace Annium.Storage.S3;

/// <summary>
/// Configuration settings for S3-compatible storage implementation
/// </summary>
public record Configuration
{
    /// <summary>
    /// The S3 server endpoint URL. Leave empty to use AWS S3
    /// </summary>
    public string Server { get; init; } = string.Empty;

    /// <summary>
    /// The access key for S3 authentication
    /// </summary>
    public string AccessKey { get; init; } = string.Empty;

    /// <summary>
    /// The secret access key for S3 authentication
    /// </summary>
    public string AccessSecret { get; init; } = string.Empty;

    /// <summary>
    /// The AWS region where the S3 bucket is located
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// The S3 bucket name where files will be stored
    /// </summary>
    public string Bucket { get; init; } = string.Empty;

    /// <summary>
    /// The directory prefix within the bucket where files will be stored
    /// </summary>
    public string Directory { get; init; } = string.Empty;
}

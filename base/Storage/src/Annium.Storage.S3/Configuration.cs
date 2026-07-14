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

    /// <summary>
    /// Whether to address the bucket by path (<c>{server}/{bucket}/{key}</c>) rather than by virtual host
    /// (<c>{bucket}.{server}/{key}</c>). Required by most S3-compatible servers (MinIO, Ceph) and by buckets whose name contains dots
    /// </summary>
    public bool ForcePathStyle { get; init; }

    /// <summary>
    /// How many keys a single list request asks for. Listing pages through the whole result regardless,
    /// so this trades round trips against response size. Defaults to 1000, the maximum S3 serves per page
    /// </summary>
    public int PageSize { get; init; } = 1000;
}

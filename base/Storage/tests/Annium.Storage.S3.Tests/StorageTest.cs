namespace Annium.Storage.S3.Tests;

/// <summary>
/// Runs the common storage suite against the S3 provider configured with a directory prefix,
/// so object keys are mapped under that prefix within the bucket.
/// </summary>
public class StorageTest : S3StorageTestBase
{
    /// <summary>
    /// The bucket this suite operates in.
    /// </summary>
    protected override string Bucket => "annium-tests";

    /// <summary>
    /// The directory prefix this suite stores under.
    /// </summary>
    protected override string Root => "/files";
}

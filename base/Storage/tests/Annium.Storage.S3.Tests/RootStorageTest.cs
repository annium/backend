namespace Annium.Storage.S3.Tests;

/// <summary>
/// Runs the common storage suite against the S3 provider configured at the bucket root,
/// exercising the key-mapping branch where object keys are used verbatim with no directory prefix.
/// </summary>
public class RootStorageTest : S3StorageTestBase
{
    /// <summary>
    /// The bucket this suite operates in. Distinct from the prefixed suite's bucket so the two stay isolated.
    /// </summary>
    protected override string Bucket => "annium-tests-root";

    /// <summary>
    /// The bucket root, meaning keys are stored without a directory prefix.
    /// </summary>
    protected override string Root => "/";
}
